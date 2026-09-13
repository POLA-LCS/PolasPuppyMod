using System;
using System.Collections.Generic;
using System.Linq;
using PuppyMod.Common.Interfaces;

namespace PuppyMod.Common.PuppySets;

public enum PuppyEquipmentKind
{
    Ears,
    Tail
}

public enum PuppyFamily
{
    None,
    Vanilla,
    Reinforced,
    Shiny
}

public enum PuppyEquipmentSlotLocation
{
    FunctionalHead,
    FunctionalAccessory,
    VanityHead,
    VanityAccessory
}

/// <summary>
/// Immutable additive contribution from one equipped Puppy item, or the aggregate of all
/// recognized entries in a snapshot. Values are full-strength functional-slot values;
/// <see cref="PuppyEquipmentEntry.ValueMultiplier"/> applies vanity scaling (0.5).
/// Uniform halving rule: All Player-stat contributions via PuppyEquipmentStats are centrally halved
/// in vanity (ValueMultiplier 0.5, pair strength 0.5); visual/physics (ShinyEars light/range,
/// ShinyTail hover 30→15) are locally halved; collar/leash are functional-only and intentionally not halved.
/// </summary>
public readonly record struct PuppyEquipmentStats(
    float Defense = 0f,
    float PickSpeed = 0f,
    float MoveSpeed = 0f,
    float AccRunSpeed = 0f,
    float MaxRunSpeed = 0f,
    float JumpSpeedBoost = 0f,
    float MeleeKnockbackAdditive = 0f,
    float SummonKnockbackFlat = 0f)
{
    public PuppyEquipmentStats Scale(float multiplier) => new(
        Defense * multiplier,
        PickSpeed * multiplier,
        MoveSpeed * multiplier,
        AccRunSpeed * multiplier,
        MaxRunSpeed * multiplier,
        JumpSpeedBoost * multiplier,
        MeleeKnockbackAdditive * multiplier,
        SummonKnockbackFlat * multiplier);

    public static PuppyEquipmentStats operator +(PuppyEquipmentStats left, PuppyEquipmentStats right) => new(
        left.Defense + right.Defense,
        left.PickSpeed + right.PickSpeed,
        left.MoveSpeed + right.MoveSpeed,
        left.AccRunSpeed + right.AccRunSpeed,
        left.MaxRunSpeed + right.MaxRunSpeed,
        left.JumpSpeedBoost + right.JumpSpeedBoost,
        left.MeleeKnockbackAdditive + right.MeleeKnockbackAdditive,
        left.SummonKnockbackFlat + right.SummonKnockbackFlat);
}

public interface IPuppyEquipmentProvider
{
    PuppyEquipmentStats Stats { get; }
}

public sealed class PuppyTooltipLineDefinition
{
    public PuppyTooltipLineDefinition(string lineName, string localizationKey, bool halveInVanity)
    {
        LineName = lineName;
        LocalizationKey = localizationKey;
        HalveInVanity = halveInVanity;
    }

    public string LineName { get; }
    public string LocalizationKey { get; }
    public bool HalveInVanity { get; }
}

public sealed class PuppyTooltipDefinition
{
    public PuppyTooltipDefinition(params PuppyTooltipLineDefinition[] lines)
    {
        Lines = Array.AsReadOnly(lines ?? Array.Empty<PuppyTooltipLineDefinition>());
    }

    public IReadOnlyList<PuppyTooltipLineDefinition> Lines { get; }
}

public abstract class PuppyEquipmentDefinition
{
    protected PuppyEquipmentDefinition(
        int itemType,
        PuppyEquipmentKind kind,
        PuppyFamily family,
        IPuppyEquipmentProvider provider,
        PuppyTooltipDefinition tooltip)
    {
        ItemType = itemType;
        Kind = kind;
        Family = family;
        Provider = provider;
        Tooltip = tooltip;
    }

    public int ItemType { get; }
    public PuppyEquipmentKind Kind { get; }
    public PuppyFamily Family { get; }
    public IPuppyEquipmentProvider Provider { get; }
    public PuppyTooltipDefinition Tooltip { get; }
}

public sealed class PuppyEarsDefinition : PuppyEquipmentDefinition
{
    public PuppyEarsDefinition(
        int itemType,
        PuppyFamily family,
        IPuppyEars provider,
        PuppyTooltipDefinition tooltip)
        : base(itemType, PuppyEquipmentKind.Ears, family, provider, tooltip)
    {
    }
}

public sealed class PuppyTailDefinition : PuppyEquipmentDefinition
{
    public PuppyTailDefinition(
        int itemType,
        PuppyFamily family,
        IPuppyTail provider,
        PuppyTooltipDefinition tooltip)
        : base(itemType, PuppyEquipmentKind.Tail, family, provider, tooltip)
    {
    }
}

public sealed class PuppyEquipmentEntry
{
    public PuppyEquipmentEntry(PuppyEquipmentDefinition definition, int slot, PuppyEquipmentSlotLocation location)
    {
        Definition = definition;
        Slot = slot;
        Location = location;
    }

    public PuppyEquipmentDefinition Definition { get; }
    public int ItemType => Definition.ItemType;
    public PuppyEquipmentKind Kind => Definition.Kind;
    public PuppyFamily Family => Definition.Family;
    public int Slot { get; }
    public PuppyEquipmentSlotLocation Location { get; }
    public bool IsFunctional => Location == PuppyEquipmentSlotLocation.FunctionalHead || Location == PuppyEquipmentSlotLocation.FunctionalAccessory;
    public bool IsAccessorySlot => Location == PuppyEquipmentSlotLocation.FunctionalAccessory || Location == PuppyEquipmentSlotLocation.VanityAccessory;
    /// <summary>Uniform halving: functional 1f, vanity 0.5f centrally for all PuppyEquipmentStats; collar/leash functional-only (no vanity).</summary>
    public float ValueMultiplier => IsFunctional ? 1f : 0.5f;

    public IPuppyEquipmentProvider Provider => Definition.Provider;
    public IPuppyEars EarsProvider => Definition.Provider as IPuppyEars;
    public IPuppyTail TailProvider => Definition.Provider as IPuppyTail;
}

/// <summary>
/// Immutable view of all recognized Puppy equipment found by the scanner for one tick.
/// </summary>
public sealed class PuppyEquipmentSnapshot
{
    public static PuppyEquipmentSnapshot Empty { get; } = new(Array.Empty<PuppyEquipmentEntry>());

    public PuppyEquipmentSnapshot(IEnumerable<PuppyEquipmentEntry> entries)
    {
        if (entries == null)
            throw new ArgumentNullException(nameof(entries));

        PuppyEquipmentEntry[] allEntries = entries.ToArray();
        Entries = Array.AsReadOnly(allEntries);
        Ears = Array.AsReadOnly(allEntries.Where(entry => entry.Kind == PuppyEquipmentKind.Ears).ToArray());
        Tails = Array.AsReadOnly(allEntries.Where(entry => entry.Kind == PuppyEquipmentKind.Tail).ToArray());
    }

    public IReadOnlyList<PuppyEquipmentEntry> Entries { get; }
    public IReadOnlyList<PuppyEquipmentEntry> Ears { get; }
    public IReadOnlyList<PuppyEquipmentEntry> Tails { get; }

    public bool ContainsFunctional(int itemType)
    {
        return Entries.Any(entry => entry.ItemType == itemType && entry.IsFunctional);
    }

    public bool ContainsVanity(int itemType)
    {
        return Entries.Any(entry => entry.ItemType == itemType && !entry.IsFunctional);
    }
}
