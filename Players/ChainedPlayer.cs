using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Physics;

namespace PuppyMod.Players;

public class ChainedPlayer : ModPlayer
{
    public int ActiveCollarItemType { get; private set; }
    public bool HasCollar => ActiveCollarItemType != 0;
    public int? GrabberIndex { get; private set; }
    public int ActiveLeashItemType { get; private set; }
    private int _overstretchTicks;

    // M10: Cached rope texture to avoid ModContent.Request each frame; refreshed when leash type changes.
    private Texture2D _cachedRopeTexture;
    private string _cachedRopePath;
    private Asset<Texture2D> _cachedRopeAsset;

    public Player AttachedOwner => OwnerOf;
    public bool HasValidAttachment => GrabberIndex.HasValue && IsChainValid();

    private float ActiveLeashRange
    {
        get
        {
            if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is IWithRange leash)
                return leash.RangePixels;
            return 0f;
        }
    }

    private int ActiveLeashRangeTiles
    {
        get
        {
            if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is IWithRange leash)
                return leash.RangeTiles;
            return 0;
        }
    }

    internal void SetCollarActive(int collarItemType)
    {
        ActiveCollarItemType = collarItemType;
    }

    internal void SetGrabberAuthority(int ownerWho, int leashItemType = 0)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        GrabberIndex = ownerWho >= 0 ? ownerWho : null;
        ActiveLeashItemType = GrabberIndex.HasValue ? leashItemType : 0;
        if (Main.netMode == NetmodeID.Server)
        {
            var mod = ModContent.GetInstance<PuppyMod>();
            if (GrabberIndex.HasValue)
                mod.BroadcastLeashState(GrabberIndex.Value, Player.whoAmI, ActiveLeashItemType, ActiveCollarItemType);
            else
                mod.BroadcastLeashDetached(Player.whoAmI);
        }
    }

    internal void ApplyClientState(int ownerWho, int leashItemType = 0, int collarItemType = 0)
    {
        if (Main.netMode == NetmodeID.Server) return;
        GrabberIndex = ownerWho == byte.MaxValue ? (int?)null : ownerWho;
        ActiveLeashItemType = GrabberIndex.HasValue ? leashItemType : 0;
        ActiveCollarItemType = collarItemType;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["ActiveCollarItemType"] = ActiveCollarItemType;
        // GrabberIndex and ActiveLeashItemType are transient (owner attachment, not persisted).
        // Do not save them; they are cleared on load/enter world and synced via net packets.
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("ActiveCollarItemType"))
            ActiveCollarItemType = tag.GetInt("ActiveCollarItemType");
        else
            ActiveCollarItemType = 0;

        GrabberIndex = null;
        ActiveLeashItemType = 0;
    }

    public override void OnEnterWorld()
    {
        GrabberIndex = null;
        ActiveLeashItemType = 0;
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        var clone = (ChainedPlayer)targetCopy;
        clone.ActiveCollarItemType = ActiveCollarItemType;
        clone.GrabberIndex = GrabberIndex;
        clone.ActiveLeashItemType = ActiveLeashItemType;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        var clone = (ChainedPlayer)clientPlayer;
        if (clone.GrabberIndex != GrabberIndex || clone.ActiveLeashItemType != ActiveLeashItemType || clone.ActiveCollarItemType != ActiveCollarItemType)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)LeashPacketType.State);
            packet.Write((byte)(GrabberIndex ?? byte.MaxValue));
            packet.Write((byte)Player.whoAmI);
            packet.Write(ActiveLeashItemType);
            packet.Write(ActiveCollarItemType);
            packet.Send();
        }
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        var packet = Mod.GetPacket();
        packet.Write((byte)LeashPacketType.State);
        packet.Write((byte)(GrabberIndex ?? byte.MaxValue));
        packet.Write((byte)Player.whoAmI);
        packet.Write(ActiveLeashItemType);
        packet.Write(ActiveCollarItemType);
        if (toWho == -1) packet.Send();
        else packet.Send(toWho);
    }

    public override void ResetEffects()
    {
        ActiveCollarItemType = 0;
    }

    public override void PostUpdateEquips()
    {
        if (HasCollar)
        {
            Lighting.AddLight(Player.Center, 0.4f, 0.3f, 0.15f);
        }
        // H2: Aggregate leash puppy effects (e.g., ChainLeash +5 defense) in PostUpdateEquips with other defense sources.
        // Order: ResetEffects → PostUpdateEquips aggregation (equipment stats + leash + pair bonus via PuppyPlayer) → physics in PostUpdate.
        // This prevents late defense flicker that occurred when leash/pair were applied in PostUpdate or ModSystem.PostUpdatePlayers.
        if (HasValidAttachment && ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leashForEquips)
            leashForEquips.AffectPuppy(Player);
    }

    public override void PostUpdate()
    {
        if (!GrabberIndex.HasValue) return;
        if (!IsChainValid())
        {
            if (Main.netMode == NetmodeID.Server)
                ModContent.GetInstance<PuppyMod>().BroadcastLeashDetached(Player.whoAmI);
            GrabberIndex = null;
            ActiveLeashItemType = 0;
            return;
        }
        // H2: AffectPuppy moved to PostUpdateEquips for defense aggregation; physics remains late (post-movement).
        ApplyLeashPhysics(OwnerOf);
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (GrabberIndex.HasValue && Main.netMode == NetmodeID.Server)
            ModContent.GetInstance<PuppyMod>().BroadcastLeashDetached(Player.whoAmI);
        GrabberIndex = null;
        ActiveLeashItemType = 0;
    }

    private void RestrictMovement(Player owner)
    {
        if (owner == null || !owner.active || owner.dead)
        {
            return;
        }
        float distance = Vector2.Distance(Player.Center, owner.Center);
        float max = ActiveLeashRange;
        if (distance <= max)
        {
            return;
        }
        Vector2 midpoint = (Player.Center + owner.Center) / 2f;
        Vector2 puppyOffset = Player.Center - midpoint;
        Vector2 ownerOffset = owner.Center - midpoint;
        float puppyPull = 0f;
        float ownerPull = 0f;
        if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leash)
        {
            puppyPull = leash.Physics.PuppyInertia;
            ownerPull = leash.Physics.OwnerInertia;
        }
        const float div = 8f;
        Player.velocity -= puppyOffset * puppyPull / div;
        owner.velocity -= ownerOffset * ownerPull / div;
    }

    private void ApplyLeashPhysics(Player owner)
    {
        if (owner == null || !owner.active || owner.dead) return;
        int rangeTiles = ActiveLeashRangeTiles;
        if (rangeTiles <= 0)
        {
            RestrictMovement(owner);
            return;
        }
        if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leash)
        {
            int tiles = leash.RangeTiles;
            if (tiles <= 0)
                tiles = rangeTiles;
            LeashPhysicsProfile profile = leash.Physics;
            var result = LeashPhysicsService.Compute(Player.Center, owner.Center, Player.velocity, owner.velocity, tiles, profile);
            if (result.Overstretched)
                _overstretchTicks++;
            else if (_overstretchTicks > 0)
                _overstretchTicks--;

            if (result.IsTaut)
            {
                Player.velocity += result.PuppyImpulse;
                owner.velocity += result.OwnerImpulse;
            }
        }
        else
        {
            RestrictMovement(owner);
        }
    }

    private bool IsChainValid()
    {
        if (!Player.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
        if (!HasCollar) return false;
        Player owner = OwnerOf;
        if (owner == null || !owner.active || owner.dead) return false;
        if (owner.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
        return true;
    }

    private Player OwnerOf
    {
        get
        {
            if (GrabberIndex is int i && i >= 0 && i < Main.player.Length)
                return Main.player[i];
            return null;
        }
    }

    private void DrawRope(Player owner)
    {
        if (Main.dedServ)
            return;
        Vector2 start = Player.Center;
        Vector2 end = owner.Center;
        Vector2 direction = end - start;
        float length = direction.Length();
        if (length <= 0f)
            return;
        direction.Normalize();
        string texPath = "Terraria/Images/Chain";
        if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leash)
            texPath = leash.LeashTexturePath;

        Texture2D ropeTexture;
        // Use cached texture if path unchanged; otherwise TryGet via Request with guard.
        if (_cachedRopePath == texPath && _cachedRopeTexture != null)
        {
            ropeTexture = _cachedRopeTexture;
        }
        else
        {
            try
            {
                if (Main.dedServ)
                    return;
                // Prefer cached asset; request with ImmediateLoad to avoid async.
                Asset<Texture2D> asset = ModContent.Request<Texture2D>(texPath, AssetRequestMode.ImmediateLoad);
                if (asset == null || !asset.IsLoaded)
                    return;
                ropeTexture = asset.Value;
                if (ropeTexture == null)
                    return;
                _cachedRopePath = texPath;
                _cachedRopeTexture = ropeTexture;
                _cachedRopeAsset = asset;
            }
            catch (Exception ex)
            {
                try { ModContent.GetInstance<PuppyMod>().Logger.Warn($"DrawRope load failed for {texPath}: {ex.Message}"); } catch { }
                return;
            }
        }

        Color ropeColor = Color.White;
        for (float i = 0; i < length; i += ropeTexture.Width)
        {
            Vector2 position = start + direction * i - Main.screenPosition;
            Main.EntitySpriteDraw(
                ropeTexture,
                position,
                null,
                ropeColor,
                direction.ToRotation() + MathHelper.PiOver2,
                new Vector2(ropeTexture.Width / 2f, ropeTexture.Height / 2f),
                1f,
                SpriteEffects.None,
                0
            );
        }
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        // M10: ModifyDrawInfo runs every draw – now lightweight; actual texture load cached above.
        if (Main.dedServ)
            return;
        if (!GrabberIndex.HasValue) return;
        Player owner = OwnerOf;
        if (owner == null || !owner.active || owner.dead) return;
        DrawRope(owner);
    }
}
