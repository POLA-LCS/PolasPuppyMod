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

public sealed class PuppyPairBonusDefinition
{
    public PuppyPairBonusDefinition(
        PuppyFamily family,
        int defenseBonus,
        float fullKnockbackMultiplier,
        string setBonusLocalizationKey = null,
        string tooltipLocalizationKey = null,
        PuppySpelunkerEffect? spelunker = null)
    {
        if (fullKnockbackMultiplier <= 0f || fullKnockbackMultiplier > 1f)
            throw new ArgumentOutOfRangeException(nameof(fullKnockbackMultiplier));

        Family = family;
        DefenseBonus = defenseBonus;
        FullKnockbackMultiplier = fullKnockbackMultiplier;
        SetBonusLocalizationKey = setBonusLocalizationKey;
        TooltipLocalizationKey = tooltipLocalizationKey;
        Spelunker = spelunker;
    }

    public PuppyFamily Family { get; }
    public int DefenseBonus { get; }
    public float FullKnockbackMultiplier { get; }
    public string SetBonusLocalizationKey { get; }
    public string TooltipLocalizationKey { get; }

    /// <summary>Optional ore-highlight effect granted by the pair; radius follows the placement state.</summary>
    public PuppySpelunkerEffect? Spelunker { get; }

    /// <summary>Uniform halving: pair strength 0.5 if either selected piece vanity; visual/physics locally halved; stats centrally via ValueMultiplier.</summary>
    public PuppyLeashBonusEffect GetEffect(PuppyEquipmentEntry selectedEars, PuppyEquipmentEntry selectedTail)
    {
        if (selectedEars == null || selectedTail == null
            || selectedEars.Family != Family || selectedTail.Family != Family)
        {
            return default;
        }

        bool isHalfStrength = !selectedEars.IsFunctional || !selectedTail.IsFunctional;
        float strength = isHalfStrength ? 0.5f : 1f;
        int defense = (int)MathF.Round(DefenseBonus * strength, MidpointRounding.AwayFromZero);
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
        int defenseBonus,
        float fullKnockbackMultiplier,
        string setBonusLocalizationKey = null,
        string tooltipLocalizationKey = null,
        PuppySpelunkerEffect? spelunker = null)
    {
        Register(new PuppyPairBonusDefinition(
            family,
            defenseBonus,
            fullKnockbackMultiplier,
            setBonusLocalizationKey,
            tooltipLocalizationKey,
            spelunker));
    }

    public static bool TryGet(PuppyFamily family, out PuppyPairBonusDefinition definition)
    {
        return definitions.TryGetValue(family, out definition);
    }

    public static void RegisterDefaults()
    {
        definitions.Clear();
        Register(PuppyFamily.Vanilla, defenseBonus: 0, fullKnockbackMultiplier: 1f);
        Register(
            PuppyFamily.Reinforced,
            defenseBonus: 2,
            fullKnockbackMultiplier: 0.8f,
            setBonusLocalizationKey: "Mods.PuppyMod.SetBonuses.ReinforcedPair",
            tooltipLocalizationKey: "Mods.PuppyMod.SetBonuses.ReinforcedPairTooltip");
        Register(
            PuppyFamily.Shiny,
            defenseBonus: 0,
            fullKnockbackMultiplier: 1f,
            spelunker: new PuppySpelunkerEffect(CostumeRadius: 5, FurryRadius: 10, TherianRadius: 15));
    }

    public static void Clear()
    {
        definitions.Clear();
    }
}
