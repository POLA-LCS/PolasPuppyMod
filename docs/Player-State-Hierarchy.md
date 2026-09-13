# Player State Hierarchy - How It Works

This document describes the split of responsibilities across the mod's player state classes. No implementation details are listed here - only what each class is responsible for and how they relate to each other.

Every connected player is tracked by three separate player state classes. Each class owns one concern and one concern only. Puppy equipment discovery is a small shared domain service, not another player state class.

```mermaid
flowchart TB
    subgraph PuppySide[PuppyPlayer]
        Equipment[Equipment snapshot/resolution]
        IsPuppy[IsPuppy derivation]
        EquipmentEffects[Aggregated equipment effects]
        SetText[Dynamic Player.setBonus text]
        BarkSys[Bark and hurt sounds]
        ClickerLoop[Clicker buff loop]
    end

    subgraph ChainedSide[ChainedPlayer]
        Collar[Active collar]
        Grabber[Grabber index]
        LeashItem[Active leash item]
        Physics[Leash physics and drawing]
    end

    subgraph OwnerSide[OwnerPlayer]
        ClickSignal[Click signal timer]
        ClickRange[Click range]
        OwnerScan[Scan leashed puppies each tick]
    end

    PuppySide --> IsPuppy
    PuppySide --> Equipment
    PuppySide --> EquipmentEffects
    PuppySide --> SetText
    PuppySide --> BarkSys
    PuppySide --> ClickerLoop
    ChainedSide --> Collar
    ChainedSide --> Grabber
    ChainedSide --> LeashItem
    ChainedSide --> Physics
    OwnerSide --> ClickSignal
    OwnerSide --> ClickRange
    OwnerSide --> OwnerScan
    OwnerScan -- reads --> Grabber
    OwnerScan -- reads --> Collar
    Physics -- reads --> IsPuppy
```

### Concepts

- The **equipment model** scans the fixed vanilla armor layout, records every recognized Ears/Tail entry, resolves the selected family pair, aggregates all individual effects with functional/vanity scaling, and derives the Puppy state. A matching pair is not required for `IsPuppy`.
- The **puppy class** owns everything about *being* a puppy: caching the equipment resolution, applying the aggregated effects, appending dynamic `Player.setBonus` text, deriving the set state, barks, hurt sounds, the Shiny Tail hover timer, and the per-tick loop that grants the Good Puppy buff when a clicker is heard. For incoming damage it also queries the leash bonus service for any attached pair effect.
- The **chained class** owns everything about *being on a leash*: which collar is active, who is holding the other end, and which leash item created the connection. It runs the leash physics, draws the rope, and maintains the attachment state used by the leash bonus service.
- The **owner class** owns everything about *being an owner of a puppy*: clicker cooldowns, click range, and the scan that applies a leashed puppy's collar effect to the owner. It does not know about barks, hurt sounds, or the puppy's own state.
- The **interplay** is deliberately narrow. The owner class reads the chained class to find puppies leashed to it. The chained class reads the puppy class to confirm eligibility. The puppy class owns clicker/bark behavior and queries the leash bonus service for attached defensive effects; it does not own or mutate the leash connection.
- A single player may simultaneously be a **puppy, a chained puppy, and an owner of other puppies** depending on which items and connections they have. Each role is tracked independently.

### Work Sequence

1. **Mod loads** - the equipment registry and three player classes are registered. Every connected player gets one instance of each player class.
2. **Puppy set is detected** - after equipment updates, the puppy class scans the fixed armor slots, caches the snapshot/resolution, aggregates every recognized entry, and exposes `IsPuppy` when any recognized Ears and any accessory Tail are present.
3. **Chained state resets** - on every tick, the chained class clears the active collar. The collar is then re-set by whichever collar item is currently in the player's accessory slot.
4. **Puppy loop runs** - the puppy class applies equipment defense, movement, pick-speed, jump and outgoing knockback effects, appends active set text through `Player.setBonus`, decrements the bark cooldown, listens for clicker signals, and applies the Good Puppy buff to any player within range of a clicker.
5. **Chained loop runs** - if the player is currently leashed, the chained class validates the connection, applies the leash's effect to the puppy, runs physics, and draws the rope. The leash bonus service can apply a selected pair's attached defense to both sides.
6. **Owner loop runs** - the owner class scans for puppies leashed to this player. For each, it applies the puppy's collar effect to the owner.
7. **Round trip completes** - all three classes have done their work. The next tick begins.
