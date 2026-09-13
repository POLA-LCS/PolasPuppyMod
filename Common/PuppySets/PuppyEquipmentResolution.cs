using System.Collections.Generic;
using System.Linq;

namespace PuppyMod.Common.PuppySets;

/// <summary>
/// Immutable result of resolving a Puppy equipment snapshot. SelectedEars determines the
/// family and SelectedTail is the one matching tail for that family's pair effects.
/// </summary>
public sealed class PuppyEquipmentResolution
{
    internal PuppyEquipmentResolution(
        PuppyEquipmentSnapshot snapshot,
        IReadOnlyList<PuppyEquipmentEntry> tails,
        PuppyEquipmentEntry selectedEars,
        PuppyEquipmentEntry selectedTail,
        PuppyEquipmentStats stats)
    {
        Snapshot = snapshot;
        Tails = tails;
        SelectedEars = selectedEars;
        SelectedTail = selectedTail;
        Stats = stats;
    }

    public static PuppyEquipmentResolution Empty { get; } = new(
        PuppyEquipmentSnapshot.Empty,
        System.Array.Empty<PuppyEquipmentEntry>(),
        null,
        null,
        default);

    public PuppyEquipmentSnapshot Snapshot { get; }
    public IReadOnlyList<PuppyEquipmentEntry> Ears => Snapshot.Ears;
    public IReadOnlyList<PuppyEquipmentEntry> Tails { get; }

    /// <summary>The highest-priority Ears entry, regardless of whether a matching Tail exists.</summary>
    public PuppyEquipmentEntry SelectedEars { get; }

    /// <summary>The only Tail selected for the Ears family's pair effects.</summary>
    public PuppyEquipmentEntry SelectedTail { get; }

    public PuppyFamily SelectedFamily => SelectedEars == null ? PuppyFamily.None : SelectedEars.Family;
    public bool HasEars => Ears.Count > 0;
    public bool HasTail => Tails.Count > 0;
    public bool HasFunctionalEars => Ears.Any(entry => entry.IsFunctional);
    public bool HasVanityEars => Ears.Any(entry => !entry.IsFunctional);
    public bool HasFunctionalTail => Tails.Any(entry => entry.IsFunctional);
    public bool HasVanityTail => Tails.Any(entry => !entry.IsFunctional);
    public bool HasPair => SelectedEars != null && SelectedTail != null;

    /// <summary>
    /// Placement of the selected pair: both functional → Therian, exactly one functional → Furry,
    /// both vanity → Costume. Based on the selected pair only; duplicate copies don't change it.
    /// </summary>
    public PuppySetPlacement SelectedPlacement => GetPlacement(SelectedEars, SelectedTail);

    public static PuppySetPlacement GetPlacement(PuppyEquipmentEntry ears, PuppyEquipmentEntry tail)
    {
        int functional = (ears?.IsFunctional == true ? 1 : 0) + (tail?.IsFunctional == true ? 1 : 0);
        return functional switch
        {
            2 => PuppySetPlacement.Therian,
            1 => PuppySetPlacement.Furry,
            _ => PuppySetPlacement.Costume
        };
    }

    public bool TryGetPairBonus(out PuppyPairBonusDefinition pairBonus)
    {
        if (!HasPair || SelectedEars.Family != SelectedTail.Family)
        {
            pairBonus = null;
            return false;
        }

        return PuppyPairBonusRegistry.TryGet(SelectedFamily, out pairBonus);
    }

    /// <summary>Canonical set state: any recognized Ears and any valid accessory Tail.</summary>
    public bool IsPuppy => HasEars && HasTail;

    /// <summary>All recognized item contributions with each entry's functional/vanity multiplier applied.</summary>
    public PuppyEquipmentStats Stats { get; }
}
