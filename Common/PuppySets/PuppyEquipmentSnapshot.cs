using System;
using System.Collections.Generic;
using System.Linq;

namespace PuppyMod.Common.PuppySets;

/// <summary>
/// Immutable view of all recognized Puppy equipment found by the scanner for one tick.
/// </summary>
public sealed class PuppyEquipmentSnapshot
{
    public static PuppyEquipmentSnapshot Empty { get; } = new([]);

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
