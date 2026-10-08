# Vanilla Item Augmentation - How It Works

This document describes how the mod enhances the vanilla dog ears, fox ears, dog tail and fox tail without replacing them. The shared Puppy set architecture, including custom Ears and Tails, is covered in [Puppy Set System](Puppy-Set-System.md); this page focuses on the vanilla-item integration.

The vanilla game has a dog ears item, a fox ears item, a dog tail item and a fox tail item. The mod leaves those items in place and augments their tooltips through GlobalItem hooks while the Puppy registry supplies their set identity and stat contributions. The fox pair is a direct clone of the dog pair: same providers, same stats, and it keeps its own vanilla texture. The result is the same vanilla item instances with extra information and Puppy behavior layered on top.

The mod also adds symmetric cross-crafting through `PuppyVanillaConversionRecipes`, so Dog Ears converts to Fox Ears and back, and Dog Tail converts to Fox Tail and back, one for one with no crafting station.

```mermaid
flowchart TB
    subgraph Vanilla[Vanilla items]
        Ears[Dog Ears and Fox Ears]
        Tail[Dog Tail and Fox Tail]
    end

    subgraph Augmentation[Mod augmentation]
        Tooltip[GlobalItem tooltip augmentation]
        Bonus[Item-tooltip bonus lines]
        Stat[Registry stat contribution]
    end

    subgraph Player[Player]
        Slots[Accessory and vanity slots]
        IsPuppy[Puppy resolution]
    end

    Ears -- "vanilla item type" --> Slots
    Tail -- "vanilla item type" --> Slots
    Slots -- "scanned with registered equipment" --> IsPuppy
    IsPuppy -- "shows in item tooltips" --> Bonus
    Slots -- "modifies" --> Stat
    Ears --> Tooltip
    Tail --> Tooltip
    Tooltip -- "merges vanity and equipable" --> Tooltip
```

### Concepts

- The mod does **not replace** the dog ears, fox ears, dog tail or fox tail. The same items the vanilla game provides continue to exist, with the same icons, the same internal type, and the same slot behavior.
- The mod **augments** those items by reacting to their vanilla item types. `DogEarsGlobalItem`, `FoxEarsGlobalItem`, `DogTailGlobalItem` and `FoxTailGlobalItem` send their tooltips through the shared Puppy tooltip helper; the equipment registry supplies their family and individual Puppy stats. The fox pair shares the vanilla providers with the dog pair, so their stats are identical.
- The tooltip is **merged**. Vanilla shows separate lines for equipable and vanity. The mod combines those lines so the player sees a single clear indication that the item works in both contexts.
- The **vanity line** is replaced with a flavor line. The vanilla "this is a vanity item" text is swapped for a friendly, on-brand message that invites the player to enjoy the puppy aesthetic.
- A **set bonus** is shown when the Puppy resolution contains any recognized Ears and any valid accessory Tail. The families do not have to match. The item tooltip explains how to bark and adds the pair line only when a matching family pair has one.
- The mod contributes to the **puppy's stats** while recognized entries are equipped. Functional entries contribute full strength; vanity entries contribute half strength. Every recognized entry contributes, not just the entries selected for a pair.
- The augmentation is **transparent**. A player who has never heard of the mod and views or equips any of these vanilla items still sees the merged tooltip and flavor line, while the puppy bonus lines appear in the item tooltip only when the local player currently has an active Puppy resolution.
- The vanilla items are still **obtainable the way vanilla intends**. The mod adds only the dog and fox conversion recipes on top and does not remove the existing way. The conversion is deliberately free and one for one, so a player who bought Dog Ears from the Zoologist can turn it into Fox Ears at any time instead of waiting for the fox moon phase. The reverse direction works the same way.
- The dog and fox pairs are **interchangeable**. Either pair satisfies the Ears and Tail set requirements, so a mixed build works the same as a matching one. The Reinforced and Shiny upgrade recipes still list Dog Ears and Dog Tail as their ingredient; a player holding only the fox variant flips it back first, which is free.

### Work Sequence

1. **Mod loads** - the vanilla item types are registered alongside the custom Puppy equipment definitions.
2. **Player views a vanilla tooltip** - the matching GlobalItem hook merges the equipable/vanity presentation, adds Puppy flavor and stat information, and keeps the vanilla item itself unchanged.
3. **Player equips or vanity-wears recognized entries** - the Puppy scanner records the armor slots and the resolver aggregates the individual effects with the functional/vanity multiplier.
4. **A valid Puppy state appears** - any recognized Ears plus any recognized accessory Tail enables the bark set state, regardless of family match.
5. **Puppy bonus lines are supplied to the item tooltip** - the relevant item tooltip shows the localized bark line and any selected pair line (`forTooltip: true`).
6. **Player uses the set bonus** - on the next allowed double-tap, the player barks. Bark audio is handled through the networked bark path: a local prediction, server validation and broadcast, and per-listener volume with distance falloff. Cosmetic reactions such as the Shiny Ears star burst remain client-side visuals.
