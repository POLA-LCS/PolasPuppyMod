# Puppy Set System - Architecture and Behavior

The Puppy set is a registry-driven equipment system. It recognizes registered Ears and Tail items, resolves the active puppy state every tick, aggregates their individual effects, and derives at most one family pair bonus.

The current registry contains the vanilla `DogEars` and `DogTail`, `ReinforcedEars` and `ReinforcedTail`, and `ShinyEars` and `ShinyTail`.

## Set detection

`PuppyPlayer` scans the vanilla armor array after equipment updates. The scanner checks these locations:

| Priority/location | Armor slots |
| --- | --- |
| Functional head | 0 |
| Functional accessory | 3-9 |
| Vanity head | 10 |
| Vanity accessory | 13-19 |

An entry is recognized only when its item type is registered as Puppy Ears or Tail. A player is a puppy when the resolution contains **any recognized Ears and any recognized Tail in a functional or vanity accessory location**. The two items do not have to belong to the same family. This `IsPuppy` state enables the bark set bonus.

The set state therefore does not require a matching pair. A nonmatching Tail can still make a player a puppy; it simply cannot provide the selected Ears family's pair effect.

## Selection and pair resolution

The resolver makes separate selections for family effects:

1. **Ears select the family.** The first Ears entry is selected in this order: functional head, functional accessory, vanity head, then vanity accessory. Slot index breaks ties within a location. The selected Ears family becomes the active family even if no Tail matches it.
2. **Tails are searched for that family.** Only accessory Tails are candidates. Matching functional Tails are preferred over matching vanity Tails, with slot index breaking ties.
3. **Family filtering happens before Tail priority.** A nonmatching functional Tail cannot block a matching vanity Tail.
4. **Only one pair is selected.** Only the selected Ears and selected matching Tail can produce a family pair bonus. Other Tails never add another pair bonus, but they still contribute their individual effects.

The selection rules and the individual-effect aggregate are independent. An item does not need to be selected for a pair to contribute its own registered stats.

## Placement states

The selected pair has a placement state that scales pair-specific effects:

| State | Condition | Shiny ore-sight radius |
| --- | --- | --- |
| Costume | both selected pieces vanity | 5 tiles |
| Furry | exactly one selected piece functional | 10 tiles |
| Therian | both selected pieces functional | 15 tiles |

Placement is derived from the selected pair only; extra duplicate copies do not change it. The state is exposed as `PuppyEquipmentResolution.SelectedPlacement`.

## Individual effects

Every recognized equipment entry contributes to one additive `PuppyEquipmentStats` aggregate. Functional entries contribute their full provider values; vanity entries contribute one half of those values. This applies to every recognized entry, so individual effects stack across multiple Ears and Tails. It is separate from the one-pair selection above.

**Uniform halving rule (centralized):**

| Category | Halving behavior | Implementation |
| --- | --- | --- |
| Player stats via `PuppyEquipmentStats` (Defense, PickSpeed, MoveSpeed, etc.) | Halved in vanity (multiplier `0.5`) | Central `PuppyEquipmentEntry.ValueMultiplier` and `PuppyPairBonusDefinition.GetEffect` strength `0.5` |
| Visual / physics (ShinyEars light intensity, ShinyTail hover) | Locally halved in the item's own code | `ShinyEarsItem` halves light intensity; `ShinyTailItem` hover `30 → 15` ticks via `PuppyPlayer` |
| Collar / leash (`CollarItem` wearer defense, `ChainLeashItem` puppy defense) | Functional-only, no vanity contribution | Applied only in functional accessory via `UpdateAccessory` / `ChainedPlayer.PostUpdateEquips`; intentionally not halved because vanity gives `0` |

Pair bonus strength is also centralized at `0.5` when either selected piece is vanity.

The current full-strength functional contributions are:

| Item | Individual contribution or effect |
| --- | --- |
| Vanilla Dog Ears | `pickSpeed -= 0.10` (faster digging) |
| Vanilla Dog Tail | `moveSpeed += 0.30`, `accRunSpeed += 0.45`, `maxRunSpeed += 0.30`, `jumpSpeedBoost += 1.0` |
| Reinforced Ears | `+2` defense; `+0.25` melee knockback; `+0.25` flat summon knockback |
| Reinforced Tail | `+2` defense; `moveSpeed += 0.20`, `accRunSpeed += 0.30`, `maxRunSpeed += 0.20`, `jumpSpeedBoost += 0.6666667` |
| Shiny Ears | `pickSpeed -= 0.12`; also emits a warm light |
| Shiny Tail | No registered stat contribution; enables the carpet hover described below |

The Reinforced Ears knockback values are outgoing effects: they are added to the player's melee and summon knockback. In vanity, the individual values are halved just like the other centralized equipment stats.

## Pair effects

The Vanilla family has no additional gameplay pair effect. A matching Shiny Ears/Tail pair grants ore sight (see Placement states): `Main.tileSpelunker` tiles inside the state radius are lit for the local player. The effect is applied by `PuppySpelunkerService` from `PuppyPlayer.PostUpdate` and is client-only; it does not exist on Shiny Ears alone. A matching Reinforced Ears/Tail pair has an attached-only effect:

- With both selected pieces functional, the attached Puppy and its Owner each gain `+2` defense and incoming knockback is multiplied by `0.8` (a 20% reduction).
- If either selected piece is vanity, the pair effect is half strength: `+1` defense and incoming knockback multiplied by `0.9` (a 10% reduction).

The leash bonus service applies the defense to both sides of a valid attachment and applies the incoming knockback reduction when either side is hurt. The effect is not active merely because the pair is equipped; the Puppy must be attached to an Owner.

## Shiny item behavior

- **Shiny Ears** provide their digging stat and a warm light in both functional and vanity use (vanity halves the light intensity). When the Puppy barks, Shiny Ears can add a local yellow star burst. Ore sight is not part of the item anymore: it belongs to the Shiny pair bonus.
- **Shiny Tail** sets the vanilla carpet flag, creating a star-particle hover platform using vanilla carpet movement. A functional Shiny Tail allows up to **30 ticks (0.5 seconds)** of hover; a vanity Shiny Tail allows **15 ticks (0.25 seconds)**. If both forms are active, the functional duration is used. Star dust beneath the player is visual feedback while the carpet is active.

## Vanilla item augmentation and set text

Vanilla `DogEars` and `DogTail` remain vanilla item instances with their original item types and defaults. `DogEarsGlobalItem` and `DogTailGlobalItem` route their tooltip changes through the shared Puppy tooltip helper; the registry separately supplies their Puppy stats and family identity. The mod does not replace either vanilla item.

The displayed set text is a dynamic fake armor-set display rather than a registered Terraria armor set. While `IsPuppy` is true, `PuppyPlayer` appends localized bark text, and any available selected-pair text, to `Player.setBonus` each equipment update. The same active lines can be inserted into the relevant item tooltip for the local player. The bark line is available for any recognized Ears plus any valid Tail, even when the families do not match.

## Authority and visual effects

Equipment is resolved from the current armor slots on each simulation; the Puppy set has no custom equipment-state packet. Normal Terraria equipment synchronization supplies the slot contents, and the server runs the same resolution for gameplay checks and stat effects.

Leash attachment is explicitly server-authoritative. A multiplayer client requests attach/detach, the server validates the owner, target, collar, held leash, and range, then broadcasts the owner, target, leash, and collar state. Clients apply the broadcast for their local view. If an attachment becomes invalid, the server broadcasts the detach state; a client does not authoritatively create the connection.

Audio and cosmetic effects stay local: bark audio is played only for the local player, and Shiny Ears lighting/star bursts plus Shiny Tail platform dust are skipped on dedicated servers and multiplayer servers. The carpet movement, equipment stats, set-state checks, and attached defense/knockback effects remain gameplay state rather than client-only decoration.
