using System.Collections.Generic;
using System.Linq;

namespace PuppyMod.Common.PuppySets;

/// <summary>
/// Immutable result of resolving a Puppy equipment snapshot.
/// SelectedEars determines the family. SelectedTail is the one matching tail for that family.
/// </summary>
public sealed class PuppyEquipmentResolution
{
    internal PuppyEquipmentResolution(
        PuppyEquipmentSnapshot snapshot,
        IReadOnlyList<PuppyEquipmentEntry> tails,
        PuppyEquipmentEntry selectedEars,
        PuppyEquipmentEntry primaryTail,
        PuppyEquipmentEntry selectedTail,
        PuppyEquipmentStats stats)
    {
        Snapshot = snapshot;
        Tails = tails;
        SelectedEars = selectedEars;
        PrimaryTail = primaryTail;
        SelectedTail = selectedTail;
        Stats = stats;
    }

    public static PuppyEquipmentResolution Empty { get; } = new(
        PuppyEquipmentSnapshot.Empty,
        System.Array.Empty<PuppyEquipmentEntry>(),
        null,
        null,
        null,
        default);

    public PuppyEquipmentSnapshot Snapshot { get; }
    public IReadOnlyList<PuppyEquipmentEntry> Ears => Snapshot.Ears;
    public IReadOnlyList<PuppyEquipmentEntry> Tails { get; }

    /// <summary>The highest-priority Ears entry, regardless of whether a matching Tail exists.</summary>
    public PuppyEquipmentEntry SelectedEars { get; }

    /// <summary>The highest-priority Tail entry, used for individual tail stats when no pair is available.</summary>
    public PuppyEquipmentEntry PrimaryTail { get; }

    /// <summary>The only Tail selected for the Ears family's pair effects.</summary>
    public PuppyEquipmentEntry SelectedTail { get; }

    /// <summary>SelectedTail when available, otherwise PrimaryTail for individual tail effects.</summary>
    public PuppyEquipmentEntry EffectiveTail => SelectedTail ?? PrimaryTail;

    public PuppyFamily SelectedFamily => SelectedEars == null ? PuppyFamily.None : SelectedEars.Family;
    public bool HasEars => Ears.Count > 0;
    public bool HasTail => Tails.Count > 0;
    public bool HasFunctionalEars => Ears.Any(entry => entry.IsFunctional);
    public bool HasVanityEars => Ears.Any(entry => !entry.IsFunctional);
    public bool HasFunctionalTail => Tails.Any(entry => entry.IsFunctional);
    public bool HasVanityTail => Tails.Any(entry => !entry.IsFunctional);
    public bool HasPair => SelectedEars != null && SelectedTail != null;

    /// <summary>Canonical set state: any recognized Ears and any valid accessory Tail.</summary>
    public bool IsPuppy => HasEars && HasTail;

    /// <summary>Individual item stats with the selected slot's functional/vanity multiplier applied.</summary>
    public PuppyEquipmentStats Stats { get; }
}
