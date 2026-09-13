using System;
using System.Collections.Generic;

namespace PuppyMod.Common.PuppySets;

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
