# Implementation Plan — PuppyMod Leash & Clicker Clean-Code Refactor

## Task Overview
- **Type:** refactor (clean code + modularization)
- **Complexity:** 8/10 — touches ModPlayer lifecycles, networking, item inheritance, buffs, and asset namespaces; high regression risk ifGod-classes split incorrectly.
- **Priority:** normal — not hotfix, but blocks future features (new leashes/clickers/collars)
- **Branch suggestion:** `refactor/leash-clicker-modularization`
- **Base commit:** `b37557a` — config split; pending moves `ChainedPlayer.cs` → `Players/`, `OwnerPlayer.cs` → `Players/` already staged as renames.

### Goals
Decouple **Leash** (owner→puppy tether + restraint + draw + net) from **Collar** (defense/light flag) and **Clicker** (owner signal → puppy buff). Remove duplication, magic numbers, naming drift, and God-classes; introduce explicit services + constants + interfaces + networking layer.

### Non-Goals
- No new gameplay balance changes (keep values, just centralize them)
- No asset pipeline changes
- No DB/migration

---

## Codebase Understanding (from crawler.findings + manual review)

### Current Physical Layout (git status = dirty)
```
PuppyMod/
├─ Players/
│  ├─ PuppyPlayer.cs      // IsPuppy, bark double-tap, HappyIfClicker(), AddStartingItems()
│  ├─ PolasBasePlayer.cs  // HasEquippedAccessoryVanity() helper
│  ├─ OwnerPlayer.cs      // ClickRange/BuffDuration/ClickSignalTimer/ClickCooldown + TriggerClick()
│  └─ ChainedPlayer.cs    // hasCollar, GrabberIndex, ActiveLeashItemType, RestrictMovement(), DrawRope(), networking
├─ Content/
│  ├─ Items/Collar/CollarItem.cs
│  ├─ Items/Clicker/BaseClickerItem.cs, ClickerItem.cs, GoldenClickerItem.cs
│  ├─ Items/Leash/BaseLeashItem.cs, ChainLeashItem.cs
│  ├─ Projectiles/ChainLeashProjectile.cs
│  ├─ Buffs/GoodPuppy/GoodPuppyBuff.cs
│  └─ GlobalItems/DogSetGlobalItem.cs
├─ PuppyMod.cs            // Packet IDs + Request/Broadcast + HandlePacket + HandleServerAttach/Detach
├─ PuppyModConfig.cs      // [LEGACY] StartingPuppySet — to delete
├─ PuppyModServerConfig.cs// EnableStartingPuppies (Header PuppySet)
├─ PuppyModClientConfig.cs// StartAsPuppy (Header PuppySet)
└─ Localization/en-US_Mods.PuppyMod.hjson
```

### Current Logical Coupling (smell map)
```
PuppyPlayer.CanHearClicker() --reads--> OwnerPlayer.ClickRange/HasClicked
            HappyIfClicker() --loops--> all players → OwnerPlayer per tick
ChainedPlayer.hasCollar (field) <--written-- CollarItem.UpdateAccessory()
ChainedPlayer.GrabberIndex / ActiveLeashItemType --written--> BaseLeashItem.UseItem() (alt-click scan) + PuppyMod net
ChainedPlayer.RestrictMovement() --reads--> BaseLeashItem.LeashRangeTiles *16f (via ModContent.GetModItem)
ChainedPlayer.PostUpdate() --calls--> BaseLeashItem.AffectPuppy() (virtual) + AddBuff(Sunflower)
BaseLeashItem.CanUseItem() --ogCached hack--> mutates Item.useStyle/useTime/useAnimation in-place
PuppyMod --owns--> all packet serialization + validation (server authority scattered)
```

---

## 1) Code Smells — Detailed Inventory

### 1A. Naming & Convention Drift
| Location | Smell | Impact |
|---|---|---|
| `ChainedPlayer.hasCollar` (public field, camelCase) | Violates C# PascalCase for public members; should be `HasCollar` property with `ResetEffects` contract. Also ambiguous vs deleted `hasChainLeash` mentioned in task — history left `MaxDistance = 15f*16f` but ChainLeash is 12 tiles. | API leak, style checker noise, search misses |
| `PuppyModConfig.StartingPuppySet` vs `PuppyModServerConfig.EnableStartingPuppies` vs `PuppyModClientConfig.StartAsPuppy` | Same concept 3 names, 2 headers identical `"PuppySet"` | Confusing for players & localization |
| `OwnerPlayer.ClickRange` is **pixels** but set from `TileRange*16f`; `BaseClickerItem.TileRange` is tiles; `BaseLeashItem.LeashRangeTiles` is tiles | Unit suffix missing | Bug magnet (`range*16` repeated 5×) |
| `PolasBasePlayer` | Unclear name; only used by `PuppyPlayer` for `HasEquippedAccessoryVanity` | Discoverability |
| `LeashState = 3` / `LeashReqAttach =1` as raw bytes in `PuppyMod` | Magic packet IDs without enum | Fragile |
| `BarksArray` + `ClicksArray` as nested static classes | Duplicated `LoadPuppySound`/`Get`/`GetRandom` logic | DRY violation |
| `ChainLeashProjectile` + `ChainLeashItem` magic numbers `0.6f +0.4f*rand`, `-2.5f`, `0.75f`, `21` segments | No named constants | Balance tuning hard |

### 1B. Duplicated Logic
- **Puppy-gate:** `if (puppy.IsPuppy) return false;` in `BaseClickerItem.CanUseItem` and `BaseLeashItem.CanUseItem` — identical guard.
- **Tooltip pattern:** `DogSetGlobalItem.ModifyTooltips` + `BaseLeashItem.ModifyTooltips` both do `FindIndex(... "Price") → Insert`; leash adds 2 lines, global adds up to 7. Should be `TooltipService`.
- **Range conversion:** `TileRange *16f` in `BaseClickerItem.UseItem` line 68, `BaseLeashItem.UseItem` line 105, `PuppyMod.HandleServerAttach` line 72, `ChainedPlayer.ActiveLeashRange` getter, `MaxDistance` constant. 5 sites.
- **Poison chance/duration:** duplicated between `ChainLeashItem` (0.20 /300) and `ChainLeashProjectile` (0.33 /300) with no shared constant.
- **Sound loading:** `BarksArray.LoadPuppySound` vs `ClicksArray.LoadPuppySound` identical except path+volume/pitch defaults.

### 1C. Magic Numbers (all should move to `PuppyConstants.cs`)
```csharp
// ChainedPlayer
const float MaxDistance = 15f * 16f;          // fallback leash length
const float puppyPull = 0.10f;                // should be 0.0125f after /8f — opaque
const float ownerPull = 0.018f;
Player.velocity -= puppyOffset * puppyPull / 8f; // why /8?
ActiveLeashRange => leash.LeashRangeTiles *16f; // repeated
Sunflower buff 60 ticks hard-coded

// OwnerPlayer — TriggerClick hard-codes
ClickSignalTimer = 10; // ticks signal lives — duplicated? Puppy checks HasClicked each tick

// PuppyPlayer
doubleTapUpTimer = 18; // frames for double-tap
barkCooldown = 20;
jumpSpeedBoost +0.75f / +0.3f, moveSpeed +0.3f/+0.15f scattered

// BaseClickerItem
Item.useTime = 20, useAnimation 20, width 32

// BaseLeashItem
Item.useTime 22/12, damage *1.25/*0.65, knockback *0.7
Tooltips color new Color(193,154,107) — 3×

 // ChainLeashItem
PoisonChance 0.20, Duration 300, RangeMultiplier 0.75f, Segments 21
 Whip shoot diff 0.6f+0.4f*rand, if NextBool(3) dir*= -2.5f
```

### 1D. God Classes & SRP Violations
- **`ChainedPlayer` (175 LOC) does 6 jobs:** Collar state (`hasCollar` + `ResetEffects` + `PostUpdateEquips` defense/light), Leash authority (`GrabberIndex`/`ActiveLeashItemType` + `SetGrabberAuthority`/`ApplyClientState`), Validations (`IsChainValid`), Physics (`RestrictMovement`), Networking (`SyncPlayer` + relies on `PuppyMod.Broadcast*`), Rendering (`DrawRope` + `ModifyDrawInfo`). Break via SRP.
- **`PuppyMod` (114 LOC) owns networking:** packet creation + broadcast + server attach/detach validation + `HandlePacket` switch. Violates SRP; should delegate to `LeashPacketHandler`.
- **`OwnerPlayer` conflates Clicker & generic owner:** `ClickRange/BuffDuration/ClickSignalTimer/ClickCooldown` are clicker-specific but will also need leash owner tracking if extended. Currently fine but naming `OwnerPlayer` suggests leash owner; split to `ClickerPlayer` or `OwnerPlayer : ClickerPlayer + LeashOwnerPlayer`.
- **`PuppyPlayer` does 4 jobs:** puppy detection (`HowPuppy` enum + armor checks), bark audio/input (`doubleTapUpTimer`, `Bark()`), starting items, clicker listening (`CanHearClicker`+`HappyIfClicker`), set bonuses (`PostUpdateMiscEffects`). `HappyIfClicker` loops all players per tick — should be service.
- **`BaseLeashItem` does 5 jobs:** item defaults, `ogCached` animation hack, leash-mode detection (`IsLeashing`), alt-click targeting (mouse hitbox + distance + net branching), tooltip, `AffectPuppy` hook.

### 1E. Design Smells
- **`ogCached` hack in `BaseLeashItem.CanUseItem`:** Mutates `Item.useStyle/useTime/useAnimation` at query time (`CanUseItem` should be pure). Relies on hidden `ogStyle/ogTime/ogAnim` fields. Correct fix: override getters or use `SetDefaults` + `UseItem` branching, or `ModItem.UseStyle` pattern; better still set `Item.useStyle = Thrust` only for alt via `CanUseItem` without caching via `AltFunctionUse` flag.
- **`ActiveLeashRange` property reaches into `ModContent.GetModItem`:** Service locator inside ModPlayer — should be `LeashService.GetRange(ActiveLeashItemType)`.
- **`CollarItem.UpdateAccessory` writes `hasCollar = true`:** Flag pattern via `ResetEffects` is tMod idiomatic but inconsistent; should be `CollarPlayer.HasCollar` or stay but document contract explicitly.
- **Distance checks use `Vector2.Distance` + `DistanceSquared` inconsistently:** `PuppyPlayer.CanHearClicker` uses `DistanceSquared` (good) but `BaseLeashItem.UseItem` and `PuppyMod.HandleServerAttach` use `Distance` (allocates sqrt).
- **`SyncPlayer` + `Broadcast*` duplication:** Manual packet serialization repeated 4× with same shape `(ownerByte, targetByte, leashTypeInt)`.
- **Before/After file locations:** `Players/` at root vs `Content/` prefix — inconsistent namespace `PuppyMod.Players` vs folder `Players/`. tMod 1.4.4 convention prefers `Content/Players` or plain `Players/` but not mixed.

---

## 2) Modularization Proposal

### 2A. File Moves (git mv, preserve history)
```
# Rename pending (already deleted at root, untracked at Players/)
git mv ChainedPlayer.cs Players/ChainedPlayer.cs   // DONE (staged delete)
git mv OwnerPlayer.cs  Players/OwnerPlayer.cs       // DONE
# New moves
git mv Players/PolasBasePlayer.cs          Common/Players/PolasBasePlayer.cs  (or Content/Common/)
git mv PuppyModConfig.cs                   (DELETE — legacy, replaced by split configs)
# Keep for now but relocate in final tree:
# Players/*.cs -> Content/Players/*.cs  (see 4)
```

### 2B. New Abstractions

#### `ILeashItem` interface (enforces contract, replaces `is BaseLeashItem` checks)
```csharp
// Content/Common/Interfaces/ILeashItem.cs
public interface ILeashItem {
    int LeashRangeTiles { get; }
    void AffectPuppy(Player puppy);
}
```

#### `ILeashItem` + `IClickerItem` unify range semantics
```csharp
public interface IRangeItem { int RangeTiles { get; } float RangePixels => RangeTiles * PuppyConstants.TileSize; }
```

#### `LeashService` (stateless pure domain)
```csharp
// Content/Services/LeashService.cs
public static class LeashService {
    public static float GetRangePixels(int itemType) => ... // fallback to PuppyConstants.DefaultLeashRangePixels
    public static bool IsValidTarget(Player owner, Player target, int leashType) { /* IsPuppy + hasCollar + distance + HeldItem */ }
    public static Player FindCursorTarget(Player owner, float rangePx) { /* Hitbox.Contains(Main.MouseWorld) loop */ }
    public static bool IsLeashing(Player owner, int leashType) { /* scan Main.player for GrabberIndex */ }
}
```

#### `ClickerService` + `CollarService`
```csharp
public static class ClickerService {
    public static void TriggerClick(OwnerPlayer o, BaseClickerItem item) => o.TriggerClick(item.RangePixels, item.BuffDuration, item.UsageCooldown);
    public static bool CanHear(Player puppy, Player owner) { /* DistanceSquared <= range² */ }
}
public static class CollarService {
    public const int DefenseBonus = 2;
    public static readonly Color LightColor = new(0.4f,0.3f,0.15f);
    public static void ApplyEffects(Player p) { p.statDefense += DefenseBonus; Lighting.AddLight(p.Center, LightColor); }
}
```

#### `LeashManager` (orchestrates attach/detach, authoritative)
```csharp
public static class LeashManager {
    public static bool TryAttach(Player owner, Player target, int leashType);
    public static bool TryDetach(Player owner, Player target);
}
```

#### `Networking/LeashPacketHandler.cs` (extracted from PuppyMod)
```csharp
public enum LeashPacketType : byte { RequestAttach=1, RequestDetach=2, StateBroadcast=3 }
public static class LeashPacketHandler {
    public static void Handle(BinaryReader r, int whoAmI);
    public static void SendAttach(int targetWho, int type);
    public static void SendDetach(int targetWho);
    public static void BroadcastState(int ownerWho,int targetWho,int type);
    public static void BroadcastDetached(int targetWho);
}
```

#### `PuppyConstants.cs`
```csharp
public static class PuppyConstants {
    public const float TileSize = 16f;
    public const float DefaultLeashRangeTiles = 15f;
    public const float DefaultLeashRangePixels = DefaultLeashRangeTiles * TileSize;
    public const int ClickSignalTicks = 10;
    public const int DoubleTapWindow = 18;
    public const int BarkCooldown = 20;
    public const float PuppyPull = 0.10f/8f;
    public const float OwnerPull = 0.018f/8f;
    // etc.
}
```

#### Extension methods
```csharp
public static class PlayerExtensions {
    public static bool IsPuppy(this Player p) => p.GetModPlayer<PuppyPlayer>().IsPuppy;
    public static bool HasCollar(this Player p) => p.GetModPlayer<ChainedPlayer>().HasCollar;
    public static bool WithinRange(this Player a, Player b, float rangePx) => Vector2.DistanceSquared(a.Center,b.Center) <= rangePx*rangePx;
    public static bool WithinTiles(this Player a, Player b, int tiles) => a.WithinRange(b, tiles*16f);
}
```

---

## 3) Clean-Code Fixes — Before/After Snippets

### 3A. Rename `hasCollar` (public field camelCase) → `HasCollar` property

**Before (`ChainedPlayer.cs:16` + `CollarItem.cs:23` + elsewhere):**
```csharp
public bool hasCollar = false;
public override void ResetEffects() { hasCollar = false; }
// CollarItem
player.GetModPlayer<ChainedPlayer>().hasCollar = true;
// IsChainValid
if (!hasCollar) return false;
// BaseLeashItem
if (!target.GetModPlayer<ChainedPlayer>().hasCollar) continue;
// PuppyMod.HandleServerAttach
if (!target.GetModPlayer<ChainedPlayer>().hasCollar) return;
```

**After:**
```csharp
// ChainedPlayer.cs
public bool HasCollar { get; set; }   // ResetEffects sets false; UpdateAccessory via CollarService
public override void ResetEffects() => HasCollar = false;

// CollarItem.cs
public override void UpdateAccessory(Player player, bool hideVisual) {
    player.GetModPlayer<ChainedPlayer>().HasCollar = true;
}

// All call sites:
if (!target.HasCollar()) return; // via extension or .GetModPlayer<ChainedPlayer>().HasCollar
```
*Verification:* `rg -n "hasCollar"` zero hits after.

### 3B. Extract `ActiveLeashRange` to `LeashService`

**Before (`ChainedPlayer.cs:20-28`):**
```csharp
private float ActiveLeashRange {
    get {
        if (ActiveLeashItemType !=0 && ModContent.GetModItem(ActiveLeashItemType) is BaseLeashItem leash)
            return leash.LeashRangeTiles *16f;
        return MaxDistance;
    }
}
```

**After:**
```csharp
// Content/Services/LeashService.cs
public static float GetLeashRangePixels(int itemType) {
    if (itemType !=0 && ModContent.GetModItem(itemType) is ILeashItem leash)
        return leash.LeashRangeTiles * PuppyConstants.TileSize;
    return PuppyConstants.DefaultLeashRangePixels;
}
// ChainedPlayer.cs
private float ActiveLeashRange => LeashService.GetLeashRangePixels(ActiveLeashItemType);
```

### 3C. Remove `ogCached` hack via proper `UseItem` + `AltFunctionUse` branching

**Before (`BaseLeashItem.cs:18-96`):**
```csharp
private int ogStyle; private int ogTime; private int ogAnim; private bool ogCached;
public override bool CanUseItem(Player player) {
    if (!ogCached && player.altFunctionUse !=2) { ogStyle=Item.useStyle; ogTime=Item.useTime; ogAnim=Item.useAnimation; ogCached=true; }
    if (player.altFunctionUse==2) { Item.useStyle=Thrust; Item.useTime=12; Item.useAnimation=12; }
    else if (ogCached) { Item.useStyle=ogStyle; Item.useTime=ogTime; Item.useAnimation=ogAnim; }
    if (IsLeashing(player)) { Item.useTime=(int)(Item.useTime*1.25f); ... }
    return base.CanUseItem(player);
}
```

**After (SRP — no mutation in query, use dedicated helpers):**
```csharp
// BaseLeashItem.cs
public override bool AltFunctionUse(Player p) => true;
public override bool CanUseItem(Player p) {
    if (p.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
    return base.CanUseItem(p);
}
public override void UseStyle(Player p, Rectangle heldItemFrame) { /* optional visual hook */ }

private void ApplyUseStats(Player p) {
    // Called from UseItem or ModifyUseTime pattern — centralize constants
    bool leashing = LeashService.IsLeashing(p, Type);
    Item.useStyle = p.altFunctionUse==2 ? ItemUseStyleID.Thrust : ItemUseStyleID.Swing;
    Item.useTime  = p.altFunctionUse==2 ? PuppyConstants.LeashAltUseTime
                  : leashing ? (int)(PuppyConstants.LeashBaseUseTime*PuppyConstants.LeashPenaltyUseTimeMult)
                             : PuppyConstants.LeashBaseUseTime;
    Item.useAnimation = Item.useTime;
    Item.damage = leashing ? (int)(BaseDamage*PuppyConstants.LeashPenaltyDamageMult) : BaseDamage;
    Item.knockBack = leashing ? BaseKnockback*PuppyConstants.LeashPenaltyKnockbackMult : BaseKnockback;
}
public override bool? UseItem(Player p) {
    ApplyUseStats(p);
    if (p.altFunctionUse==2) return LeashManager.TryToggleFromCursor(p, this);
    return true;
}
```
*Alternative tMod-idiomatic:* override `UseTimeMultiplier`/`UseAnimationMultiplier` for penalty instead of mutating Item.

### 3D. Introduce `ILeashItem` interface

**Before:** `if (ModContent.GetModItem(ActiveLeashItemType) is BaseLeashItem leash)` — couples to base class.

**After:**
```csharp
public interface ILeashItem { int LeashRangeTiles { get; } void AffectPuppy(Player p); }
public abstract class BaseLeashItem : ModItem, ILeashItem { ... }
...
if (ModContent.GetModItem(type) is ILeashItem leash) { leash.AffectPuppy(p); }
```
Enables future non-`BaseLeashItem` leashes.

### 3E. Extract `LeashManager` + `LeashPacketHandler` from `PuppyMod` + `BaseLeashItem.UseItem`

**Before (`PuppyMod.cs:61-87` + `BaseLeashItem.cs:100-138`):** inline loops + packet writes.

**After:**
```csharp
// LeashManager.cs
public static bool TryToggleFromCursor(Player owner, ILeashItem leash) {
    var target = LeashService.FindCursorTarget(owner, leash.LeashRangeTiles*16f);
    if (target==null) return false;
    var chain = target.GetModPlayer<ChainedPlayer>();
    bool ownedByMe = chain.GrabberIndex==owner.whoAmI;
    if (ownedByMe) {
        if (Main.netMode==NetmodeID.MultiplayerClient) LeashPacketHandler.SendDetach(target.whoAmI);
        else { chain.SetGrabberAuthority(-1,0); LeashPacketHandler.BroadcastDetached(target.whoAmI); }
    } else {
        if (chain.GrabberIndex.HasValue) return false;
        if (Main.netMode==NetmodeID.MultiplayerClient) LeashPacketHandler.SendAttach(target.whoAmI, ((ModItem)leash).Type);
        else { chain.SetGrabberAuthority(owner.whoAmI, ((ModItem)leash).Type); LeashPacketHandler.BroadcastState(owner.whoAmI,target.whoAmI, ((ModItem)leash).Type); }
    }
    return true;
}
```

### 3F. Split `ChainedPlayer` God class (SRP)

**Before:** 6 responsibilities in one file.

**After — 3 files + 1 service:**
```
Players/ChainedPlayer.cs          // GrabberIndex, ActiveLeashItemType, SyncPlayer, Kill, PostUpdate (orchestration only)
Players/CollarPlayer.cs           // HasCollar, ResetEffects, PostUpdateEquips (defense/light) — or keep in ChainedPlayer but documented
Services/LeashPhysicsService.cs   // RestrictMovement(owner, puppy, range) pure
Services/LeashDrawService.cs      // DrawRope(start,end, texture, color)
Common/Extensions/RangeExtensions.cs // WithinRange helpers
```
*If keeping single ModPlayer for save-compat:* extract private helpers to `partial class ChainedPlayer` files: `ChainedPlayer.Net.cs`, `ChainedPlayer.Physics.cs`, `ChainedPlayer.Draw.cs`.

### 3G. `OwnerPlayer` — clarify Clicker vs Leash ownership

**Before:** `OwnerPlayer` holds clicker state but name implies leash owner too.

**After options:**
- **Option A (preferred, minimal):** Rename to `ClickerPlayer` + keep `OwnerPlayer` as alias `[Obsolete]` for save compat, or make `OwnerPlayer : ModPlayer` aggregate both: add `LeashOwnerState` struct for future.
- **Option B:** Keep `OwnerPlayer` but extract `ClickerState` struct:
```csharp
public struct ClickerState { public float RangePx; public int BuffDuration; public int SignalTimer; public int Cooldown; }
public class OwnerPlayer : ModPlayer { public ClickerState Clicker; public bool IsLeashing => LeashService.IsLeashing(Player, ...); }
```

### 3H. Tooltip duplication → `TooltipService`

**Before:** `BaseLeashItem.ModifyTooltips` and `DogSetGlobalItem.ModifyTooltips` each manually `FindIndex("Price")`.

**After:**
```csharp
public static class TooltipService {
    public static void InsertAfterPrice(List<TooltipLine> tips, TooltipLine line) { int idx=tips.FindIndex(l=>l.Name=="Price"&&l.Mod=="Terraria"); if(idx>=0) tips.Insert(idx,line); else tips.Add(line); }
    public static void AddLeashTooltips(List<TooltipLine> tips, Mod mod, int rangeTiles) { ... }
}
```

### 3I. Config naming consolidation

**Before:** `StartingPuppySet` (legacy) / `EnableStartingPuppies` / `StartAsPuppy` with duplicate `PuppySet` headers.

**After:**
```csharp
// PuppyModServerConfig.cs  [Header("StartingSet")]
[DefaultValue(true)] public bool EnableStartingPuppies; // server wins — kept
// PuppyModClientConfig.cs  [Header("StartingSet")]
[DefaultValue(true)] public bool StartAsPuppy; // client pref — kept, but add Tooltip: "Requires server EnableStartingPuppies"
// DELETE PuppyModConfig.cs entirely (already superseded)
```
Add `[Label]` attributes explicitly, update `en-US` hjson.

### 3J. `ResetEffects` correct usage + `ModPlayer` lifecycle

**Current:** `ChainedPlayer.ResetEffects` correctly clears `hasCollar`; `OwnerPlayer` does not override (OK). `PuppyPlayer` computes `HowPuppy` in `PostUpdateEquips` — correct. Document contract in `CollarService`.

### 3K. Distance extensions

**Before:**
```csharp
Vector2.Distance(player.Center, target.Center) > rangePx // sqrt
```

**After:**
```csharp
if (!owner.WithinRange(target, rangePx)) continue;
if (!puppy.CanHearClicker(owner)) // internally DistanceSquared
```

---

## 4) File Structure After Cleanup

### Target Tree (proposed, tModLoader 1.4.4 conventions)

```
PuppyMod/
├─ .opencode/
│  └─ TODO.md                        // this plan
├─ build.txt
├─ PuppyMod.cs                       // thin Mod entry; delegates to LeashPacketHandler
├─ PuppyMod.csproj
├─ Assets/
│  ├─ Barks/ (woof*.wav)
│  └─ Clicks/ (clicker*.wav)
├─ Localization/
│  └─ en-US_Mods.PuppyMod.hjson
├─ Content/                          // all ModContent
│  ├─ Common/
│  │  ├─ Constants/
│  │  │  └─ PuppyConstants.cs        // TileSize, ranges, pull factors, buff times, colors
│  │  ├─ Interfaces/
│  │  │  ├─ ILeashItem.cs
│  │  │  └─ IClickerItem.cs
│  │  ├─ Extensions/
│  │  │  ├─ PlayerExtensions.cs      // IsPuppy(), HasCollar(), WithinRange()
│  │  │  └─ TooltipExtensions.cs
│  │  └─ Services/
│  │     ├─ SoundService.cs          // unified LoadPuppySound / GetRandom
│  │     └─ TooltipService.cs
│  ├─ Players/
│  │  ├─ PuppyPlayer.cs              // IsPuppy detection + set bonuses + bark I/O
│  │  ├─ CollarPlayer.cs             // OR keep HasCollar in ChainedPlayer but extracted partial
│  │  ├─ ChainedPlayer.cs            // leash attach state + networking + orchestration
│  │  ├─ ChainedPlayer.Physics.cs    // (partial) RestrictMovement
│  │  ├─ ChainedPlayer.Draw.cs       // (partial) DrawRope
│  │  ├─ OwnerPlayer.cs              // ClickerState (rename to ClickerPlayer later)
│  │  └─ PolasBasePlayer.cs          // -> move here from root Players/; could merge into PlayerExtensions
│  ├─ Services/                      // domain services (pure)
│  │  ├─ LeashService.cs
│  │  ├─ LeashManager.cs
│  │  ├─ LeashPhysicsService.cs
│  │  ├─ LeashDrawService.cs
│  │  ├─ ClickerService.cs
│  │  └─ CollarService.cs
│  ├─ Networking/
│  │  └─ LeashPacketHandler.cs       // enum LeashPacketType, Handle, Send/Broadcast
│  ├─ Items/
│  │  ├─ Collar/
│  │  │  └─ CollarItem.cs
│  │  ├─ Clicker/
│  │  │  ├─ BaseClickerItem.cs       // implements IClickerItem
│  │  │  ├─ ClickerItem.cs
│  │  │  └─ GoldenClickerItem.cs
│  │  └─ Leash/
│  │     ├─ BaseLeashItem.cs         // implements ILeashItem
│  │     └─ ChainLeashItem.cs
│  ├─ Projectiles/
│  │  └─ ChainLeashProjectile.cs
│  ├─ Buffs/
│  │  └─ GoodPuppy/
│  │     └─ GoodPuppyBuff.cs
│  └─ GlobalItems/
│     └─ DogSetGlobalItem.cs
├─ Configs/                          // OPTIONAL: move configs here for clarity
│  ├─ PuppyServerConfig.cs           // (or keep at root — tMod scans both)
│  └─ PuppyClientConfig.cs
└─ Players/                          // DELETE after move — kept at root for now due to git history,
                                     // final should be Content/Players only
```

**Namespace mapping:**
- `PuppyMod.Players` → `PuppyMod.Content.Players` (or keep `PuppyMod.Players` for save compat via `Type` alias — add `[Autoload(false)]` dance not needed; ModPlayer save is by type name, so rename requires testing. Safer to keep namespace `PuppyMod.Players` even if file moves to `Content/Players/`.)
- `PuppyMod.Content.Items.*` stays
- `PuppyMod.Content.Services.*`, `PuppyMod.Content.Networking.*`, `PuppyMod.Content.Common.*`

**If strict SRP split without breaking saves:** use `partial`:
```
Content/Players/ChainedPlayer.cs         // main partial
Content/Players/ChainedPlayer.Net.cs     // Sync, ApplyClientState, SetGrabberAuthority
Content/Players/ChainedPlayer.Physics.cs // RestrictMovement
Content/Players/ChainedPlayer.Draw.cs    // DrawRope
```

---

## 5) Prioritized TODO List — Refactor Steps

### Phase 0 — Prep & Safety (no behavior change)
- [ ] **0.1 Create branch + ensure `.opencode/` exists** — `mkdir -p .opencode`; commit this TODO.md
  - Agent: planner
  - Verification: `Test-Path .opencode/TODO.md`
- [ ] **0.2 Finish pending `git mv`** — confirm `Players/ChainedPlayer.cs` and `Players/OwnerPlayer.cs` staged correctly; `git status` clean except intended mods
  - Agent: refactor-agent
  - Dependencies: none
  - Verification: `git diff --name-status` shows `R100`
- [ ] **0.3 Add `PuppyConstants.cs` skeleton** — extract all magic numbers as `const` without wiring yet
  - Agent: refactor-agent
  - Verification: compiles

### Phase 1 — Low-risk Extracts (pure, no net)
- [ ] **1.1 Create `PuppyConstants.cs` + `PlayerExtensions.cs`** — centralize `TileSize=16f`, `DefaultLeashRangePixels`, `LeashPenalty*`, `ClickSignalTicks=10`, `DoubleTapWindow=18`, `CollarDefense=2`, `RopeColor`, etc. Add `WithinRange` extensions
  - Agent: refactor-agent
  - Verification: `rg "16f|*16"` shrinks to single definition
- [ ] **1.2 Create `SoundService.cs` to unify `BarksArray`/`ClicksArray`** — single `LoadSound(path, vol, pitchVar)` + `GetRandom`
  - Agent: refactor-agent
  - Verification: both arrays delegate to service
- [ ] **1.3 Create `TooltipService.cs`** — extract `InsertAfterPrice` helper; refactor both `BaseLeashItem` and `DogSetGlobalItem`
  - Agent: refactor-agent
  - Verification: tooltips visually identical

### Phase 2 — Interface & Service Layer (behavior preserving)
- [ ] **2.1 Introduce `ILeashItem` + `IClickerItem`** — make `BaseLeashItem`/`BaseClickerItem` implement; update `ChainedPlayer.ActiveLeashRange` + `PuppyMod.HandleServerAttach` to use interface
  - Agent: refactor-agent
  - Dependencies: 1.1
  - Verification: `is ILeashItem` check passes for `ChainLeashItem`
- [ ] **2.2 Extract `LeashService`** — `GetLeashRangePixels`, `IsLeashing`, `FindCursorTarget`, `IsValidTarget`
  - Agent: refactor-agent
  - Verification: unit testable (mock players)
- [ ] **2.3 Extract `ClickerService` + `CollarService`** — move `CollarPlayer.HasCollar` logic + `HappyIfClicker` loop
  - Agent: refactor-agent
  - Verification: `PuppyPlayer.HappyIfClicker` delegates
- [ ] **2.4 Rename `hasCollar` → `HasCollar`** — property + fix all 4 call sites + add extension `HasCollar(this Player)`
  - Agent: refactor-agent
  - Dependencies: 2.3
  - Verification: `rg hasCollar` zero hits; `ResetEffects` documented

### Phase 3 — God-Class Split (medium risk)
- [ ] **3.1 Split `ChainedPlayer`** — keep orchestration in main file, move `RestrictMovement` → `LeashPhysicsService`, `DrawRope` → `LeashDrawService`
  - Agent: refactor-agent
  - Dependencies: 1.1, 2.2
  - Verification: physics/draw identical; `PostUpdate` 5 lines
- [ ] **3.2 Refactor `BaseLeashItem` — remove `ogCached` hack** — implement `ApplyUseStats` / `LeashManager.TryToggleFromCursor` pattern; no `Item` mutation in `CanUseItem`
  - Agent: refactor-agent
  - Dependencies: 2.2
  - Verification: alt-click tether still works single + MP; weapon penalty applies only when leashing
- [ ] **3.3 Split `OwnerPlayer` concern** — extract `ClickerState` struct; optionally rename to `ClickerPlayer` with `[Obsolete] OwnerPlayer` alias
  - Agent: refactor-agent
  - Verification: `TriggerClick` / `HasClicked` unchanged
- [ ] **3.4 Refactor `PuppyPlayer`** — extract `HappyIfClicker` → `ClickerService.ApplyPuppyBuffIfHeard`, keep bark double-tap isolated
  - Agent: refactor-agent
  - Verification: puppy still buffs in range only

### Phase 4 — Networking Extraction (high risk, needs MP test)
- [ ] **4.1 Create `LeashPacketHandler.cs` + `LeashPacketType` enum** — move all `GetPacket`/`Write`/`Send` + `HandleServerAttach/Detach` from `PuppyMod` + `ChainedPlayer.SyncPlayer`/`SetGrabberAuthority`/`ApplyClientState` delegation
  - Agent: network-agent
  - Dependencies: 2.2, 3.1
  - Verification: `PuppyMod.HandlePacket` is 3-line switch delegating
- [ ] **4.2 Centralize packet serialization shape** — single helper `WriteLeashState(owner, target, type)`; replace duplication in `SyncPlayer`, `BroadcastLeashState`, `BroadcastLeashDetached`
  - Agent: network-agent
  - Verification: packet size stable; sniff with `ModPacket` log
- [ ] **4.3 MP manual test matrix** — Single → Client→Server attach/detach, attach stolen rejection, collar unequip auto-detach, death detach, sync for late joiner (`SyncPlayer`), range edge (12 vs 15 tiles)
  - Agent: qa
  - Verification: checklist passes on local MP (host + client)

### Phase 5 — File Structure & Config Cleanup
- [ ] **5.1 Move `Players/*.cs` → `Content/Players/*.cs` (keep namespace for save compat)** — `git mv` + update `using`
  - Agent: refactor-agent
  - Verification: tMod builds, existing worlds load puppies
- [ ] **5.2 Move `PolasBasePlayer.cs` → `Content/Common/Players/` or merge into `PlayerExtensions`** — deprecate ambiguous name
  - Agent: refactor-agent
  - Verification: no external references broken
- [ ] **5.3 Delete `PuppyModConfig.cs` (legacy `StartingPuppySet`)** — already replaced by split configs; update `build.txt` if referenced
  - Agent: refactor-agent
  - Verification: `PuppyPlayer.AddStartingItems` reads server/client correctly (server wins)
- [ ] **5.4 Normalize config headers & localization** — `EnableStartingPuppies` + `StartAsPuppy` share `StartingSet` header; update hjson; add `Tooltip` about server precedence
  - Agent: refactor-agent
  - Verification: in-game config UI shows consistent names
- [ ] **5.5 Create folders `Content/Services`, `Content/Networking`, `Content/Common` per tree above** — wire namespaces, `global using` if needed
  - Agent: refactor-agent
  - Verification: `dotnet build` no namespace collisions

### Phase 6 — Polish & Docs
- [ ] **6.1 Replace all `Distance` with `DistanceSquared` via extensions** — `WithinRange`/`WithinTiles`
  - Agent: refactor-agent
  - Verification: `rg "Vector2.Distance\("` zero hits (except physics lerp)
- [ ] **6.2 Consolidate whips/leash constants** — move `WhipSettings`, `PoisonChance` to `PuppyConstants`; unify `ChainLeashItem` vs `ChainLeashProjectile` poison logic
  - Agent: refactor-agent
  - Verification: single source for poison values
- [ ] **6.3 Add XML docs for `ResetEffects`/`PostUpdateEquips` contracts** — why Collar flag pattern, when `HasCollar` resets
  - Agent: docs-agent
- [ ] **6.4 Update `README.md` + `description_workshop.txt` with new structure** — note `ILeashItem` for modders
  - Agent: docs-agent

---

## Checkpoints
- [ ] Phase 0 complete — branch + constants skeleton + git mv clean
- [ ] Phase 1 complete — pure extracts (constants/extensions/sound/tooltip) without behavior change
- [ ] Phase 2 complete — interfaces + services + rename `HasCollar`
- [ ] Phase 3 complete — God-class splits + `ogCached` removal
- [ ] Phase 4 complete — networking extracted + MP matrix green
- [ ] Phase 5 complete — file tree matches target + legacy config deleted
- [ ] Phase 6 complete — polish + docs + README

---

## Verification Per Step (global)
- **Build:** `dotnet build PuppyMod.csproj` (or `tModLoader` build) after every phase
- **Single-player:** collar equip/unequip + leash alt-click tether/detach at 12 tiles vs 15 fallback; clicker 10 vs 15 range + buff durations 180/240
- **Multiplayer:** host + 1 client, test attach stolen-block, death detach, late-join sync (`SyncPlayer`), range validation server-authoritative
- **Style:** `rg -n "hasCollar|ogCached|16f|\* 16"` reflects progress; no new `hasChainLeash` drift
- **No regressions:** `git diff -w` per phase reviewed; no balance change (defense +2 collar, +5 chain leash, poison 20%/33% retained but centralized)

---

## Risks & Mitigations
- **ModPlayer rename breaks saves:** Mitigate by keeping namespace `PuppyMod.Players` via `using` alias; test world load with puppy equipped.
- **`ogCached` removal changes Item animation:** Mitigate by replicating exact `useTime` values from constants and testing both alt vs swing with/without leashing.
- **Packet refactor desync:** Mitigate by keeping `LeashState` byte ID 3 stable; add `ModPacket` logging temporarily.
- **Distance Squared change:** Ensure `WithinRange` uses `<=` same as old `Distance <= max`.

---

## Estimated Steps & Dependencies
- **Total steps:** 22 sub-tasks across 6 phases
- **Critical path:** 0.2 → 1.1 → 2.2 → 3.2 → 4.1 → 4.3
- **Parallelizable:** 1.2 (sound) + 1.3 (tooltip) after 0.1; 3.1 + 3.3 after 2.2
- **planner.todo_items:** `[0.1,0.2,0.3,1.1,1.2,1.3,2.1,2.2,2.3,2.4,3.1,3.2,3.3,3.4,4.1,4.2,4.3,5.1,5.2,5.3,5.4,5.5,6.1,6.2,6.3,6.4]`
- **planner.dependencies:** `{"1.1":["0.3"],"2.1":["1.1"],"2.2":["1.1"],"2.3":["1.1"],"2.4":["2.3"],"3.1":["1.1","2.2"],"3.2":["2.2"],"4.1":["2.2","3.1"],"4.2":["4.1"],"4.3":["4.1","4.2"],"5.1":["3.1","3.4"],"6.1":["1.1","2.2"]}`
- **planner.estimated_steps:** 22 (plus 4 docs/polish = 26)

---

## Appendix — Before/After File Tree Diff

```diff
- ./ChainedPlayer.cs (root, deleted)
- ./OwnerPlayer.cs    (root, deleted)
- ./PuppyModConfig.cs (legacy)
+ ./Content/Common/Constants/PuppyConstants.cs
+ ./Content/Common/Interfaces/ILeashItem.cs
+ ./Content/Common/Interfaces/IClickerItem.cs
+ ./Content/Common/Extensions/PlayerExtensions.cs
+ ./Content/Common/Services/SoundService.cs
+ ./Content/Common/Services/TooltipService.cs
  ./Content/Items/... (unchanged paths but now implement interfaces)
+ ./Content/Services/LeashService.cs
+ ./Content/Services/LeashManager.cs
+ ./Content/Services/LeashPhysicsService.cs
+ ./Content/Services/LeashDrawService.cs
+ ./Content/Services/ClickerService.cs
+ ./Content/Services/CollarService.cs
+ ./Content/Networking/LeashPacketHandler.cs
  ./Content/Players/PuppyPlayer.cs (slimmed)
  ./Content/Players/ChainedPlayer.cs (slimmed, + partials)
  ./Content/Players/OwnerPlayer.cs (ClickerState)
  ./Content/Players/PolasBasePlayer.cs (moved)
  ./PuppyMod.cs (thin, delegates)
  ./PuppyModServerConfig.cs (header fix)
  ./PuppyModClientConfig.cs (header fix)
```

---

*Generated 2026-08-30 — planner agent. Update progressively as tasks close.*
