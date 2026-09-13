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
/// Base values supplied by an ears item. Slot scaling is applied by the equipment entry.
/// </summary>
public readonly record struct PuppyEarsStats(float PickSpeed)
{
    public PuppyEarsStats Scale(float multiplier) => new(PickSpeed * multiplier);
}

/// <summary>
/// Base values supplied by a tail item. Slot scaling is applied by the equipment entry.
/// </summary>
public readonly record struct PuppyTailStats(
    float MoveSpeed,
    float AccRunSpeed,
    float MaxRunSpeed,
    float JumpSpeedBoost)
{
    public PuppyTailStats Scale(float multiplier) => new(
        MoveSpeed * multiplier,
        AccRunSpeed * multiplier,
        MaxRunSpeed * multiplier,
        JumpSpeedBoost * multiplier);
}

public readonly record struct PuppyEquipmentStats(
    float PickSpeed,
    float MoveSpeed,
    float AccRunSpeed,
    float MaxRunSpeed,
    float JumpSpeedBoost);

public interface IPuppyEquipmentProvider
{
}

public abstract class PuppyEquipmentDefinition
{
    protected PuppyEquipmentDefinition(int itemType, PuppyEquipmentKind kind, PuppyFamily family)
    {
        ItemType = itemType;
        Kind = kind;
        Family = family;
    }

    public int ItemType { get; }
    public PuppyEquipmentKind Kind { get; }
    public PuppyFamily Family { get; }
}

public sealed class PuppyEarsDefinition : PuppyEquipmentDefinition
{
    public PuppyEarsDefinition(int itemType, PuppyFamily family, IPuppyEars provider)
        : base(itemType, PuppyEquipmentKind.Ears, family)
    {
        Provider = provider;
    }

    public IPuppyEars Provider { get; }
}

public sealed class PuppyTailDefinition : PuppyEquipmentDefinition
{
    public PuppyTailDefinition(int itemType, PuppyFamily family, IPuppyTail provider)
        : base(itemType, PuppyEquipmentKind.Tail, family)
    {
        Provider = provider;
    }

    public IPuppyTail Provider { get; }
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
    public float ValueMultiplier => IsFunctional ? 1f : 0.5f;

    public IPuppyEars EarsProvider => (Definition as PuppyEarsDefinition)?.Provider;
    public IPuppyTail TailProvider => (Definition as PuppyTailDefinition)?.Provider;
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
