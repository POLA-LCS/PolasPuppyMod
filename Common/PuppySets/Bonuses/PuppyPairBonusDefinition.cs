using System;
using PuppyMod.Common.PuppySets.Core;
using PuppyMod.Common.PuppySets.Definitions;

namespace PuppyMod.Common.PuppySets.Bonuses;

public sealed class PuppyPairBonusDefinition
{
    public PuppyPairBonusDefinition(
        PuppyFamily family,
        float fullKnockbackMultiplier,
        string setBonusLocalizationKey = null,
        string tooltipLocalizationKey = null,
        PuppySpelunkerEffect? spelunker = null,
        PuppyDefenseEffect? placementDefense = null)
    {
        if (fullKnockbackMultiplier <= 0f || fullKnockbackMultiplier > 1f)
            throw new ArgumentOutOfRangeException(nameof(fullKnockbackMultiplier));

        Family = family;
        FullKnockbackMultiplier = fullKnockbackMultiplier;
        SetBonusLocalizationKey = setBonusLocalizationKey;
        TooltipLocalizationKey = tooltipLocalizationKey;
        Spelunker = spelunker;
        PlacementDefense = placementDefense;
    }

    public PuppyFamily Family { get; }
    public float FullKnockbackMultiplier { get; }
    public string SetBonusLocalizationKey { get; }
    public string TooltipLocalizationKey { get; }

    /// <summary>Optional ore-highlight effect granted by the pair; radius follows the placement state.</summary>
    public PuppySpelunkerEffect? Spelunker { get; }

    /// <summary>Optional defense granted by the pair; value follows the placement state.</summary>
    public PuppyDefenseEffect? PlacementDefense { get; }

    /// <summary>
    /// Pair effect for the selected pieces. Defense comes from <see cref="PlacementDefense"/> when set;
    /// incoming knockback is reduced at full strength only in the Therian state.
    /// </summary>
    public PuppyLeashBonusEffect GetEffect(PuppyEquipmentEntry selectedEars, PuppyEquipmentEntry selectedTail)
    {
        if (selectedEars == null || selectedTail == null
            || selectedEars.Family != Family || selectedTail.Family != Family)
        {
            return default;
        }

        PuppySetPlacement placement = PuppySetPlacementHelper.FromEntries(selectedEars, selectedTail);
        int defense = PlacementDefense?.GetDefense(placement) ?? 0;
        float strength = placement == PuppySetPlacement.Therian ? 1f : 0.5f;
        float knockbackMultiplier = 1f - ((1f - FullKnockbackMultiplier) * strength);
        return new PuppyLeashBonusEffect(defense, knockbackMultiplier);
    }
}
