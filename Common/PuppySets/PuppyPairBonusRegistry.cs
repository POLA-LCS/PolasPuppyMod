using System;
using System.Collections.Generic;

namespace PuppyMod.Common.PuppySets;

public readonly record struct PuppyLeashBonusEffect(
    int DefenseBonus,
    float KnockbackMultiplier)
{
    public bool HasEffect => DefenseBonus != 0 || KnockbackMultiplier < 1f;

    public bool IsStrongerThan(PuppyLeashBonusEffect other)
    {
        if (DefenseBonus != other.DefenseBonus)
            return DefenseBonus > other.DefenseBonus;

        return KnockbackMultiplier < other.KnockbackMultiplier;
    }
}

/// <summary>Ore-highlight radii granted by a set pair, chosen by the pair's placement state.</summary>
public readonly record struct PuppySpelunkerEffect(
    int CostumeRadius,
    int FurryRadius,
    int TherianRadius)
{
    public int GetRadius(PuppySetPlacement placement) => placement switch
    {
        PuppySetPlacement.Therian => TherianRadius,
        PuppySetPlacement.Furry => FurryRadius,
        _ => CostumeRadius
    };
}

/// <summary>Defense granted by a set pair, chosen by the pair's placement state.</summary>
public readonly record struct PuppyDefenseEffect(
    int CostumeDefense,
    int FurryDefense,
    int TherianDefense)
{
    public int GetDefense(PuppySetPlacement placement) => placement switch
    {
        PuppySetPlacement.Therian => TherianDefense,
        PuppySetPlacement.Furry => FurryDefense,
        _ => CostumeDefense
    };
}

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

        PuppySetPlacement placement = PuppyEquipmentResolution.GetPlacement(selectedEars, selectedTail);
        int defense = PlacementDefense?.GetDefense(placement) ?? 0;
        float strength = placement == PuppySetPlacement.Therian ? 1f : 0.5f;
        float knockbackMultiplier = 1f - ((1f - FullKnockbackMultiplier) * strength);
        return new PuppyLeashBonusEffect(defense, knockbackMultiplier);
    }
}

public static class PuppyPairBonusRegistry
{
    private static readonly Dictionary<PuppyFamily, PuppyPairBonusDefinition> definitions = new();

    public static void Register(PuppyPairBonusDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        definitions[definition.Family] = definition;
    }

    public static void Register(
        PuppyFamily family,
        float fullKnockbackMultiplier,
        string setBonusLocalizationKey = null,
        string tooltipLocalizationKey = null,
        PuppySpelunkerEffect? spelunker = null,
        PuppyDefenseEffect? placementDefense = null)
    {
        Register(new PuppyPairBonusDefinition(
            family,
            fullKnockbackMultiplier,
            setBonusLocalizationKey,
            tooltipLocalizationKey,
            spelunker,
            placementDefense));
    }

    public static bool TryGet(PuppyFamily family, out PuppyPairBonusDefinition definition)
    {
        return definitions.TryGetValue(family, out definition);
    }

    public static void RegisterDefaults()
    {
        definitions.Clear();
        Register(PuppyFamily.Vanilla, fullKnockbackMultiplier: 1f);
        Register(
            PuppyFamily.Reinforced,
            fullKnockbackMultiplier: 0.8f,
            setBonusLocalizationKey: "Mods.PuppyMod.SetBonuses.ReinforcedPair",
            tooltipLocalizationKey: "Mods.PuppyMod.SetBonuses.ReinforcedPairTooltip",
            placementDefense: new PuppyDefenseEffect(CostumeDefense: 2, FurryDefense: 3, TherianDefense: 4));
        Register(
            PuppyFamily.Shiny,
            fullKnockbackMultiplier: 1f,
            spelunker: new PuppySpelunkerEffect(CostumeRadius: 5, FurryRadius: 10, TherianRadius: 15));
    }

    public static void Clear()
    {
        definitions.Clear();
    }
}
