using System.Collections.Generic;
using Terraria;
using Terraria.Localization;

namespace PuppyMod.Common.PuppySets;

public static class PuppySetBonusText
{
    public const string BarkLocalizationKey = "Mods.PuppyMod.SetBonuses.Bark";
    public const string BarkDirectionUpLocalizationKey = "Mods.PuppyMod.SetBonuses.BarkDirectionUp";
    public const string BarkDirectionDownLocalizationKey = "Mods.PuppyMod.SetBonuses.BarkDirectionDown";

    public const string SpelunkerCostumeLocalizationKey = "Mods.PuppyMod.SetBonuses.SpelunkerCostume";
    public const string SpelunkerFurryLocalizationKey = "Mods.PuppyMod.SetBonuses.SpelunkerFurry";
    public const string SpelunkerTherianLocalizationKey = "Mods.PuppyMod.SetBonuses.SpelunkerTherian";

    public static IEnumerable<string> GetActiveLines(
        PuppyEquipmentResolution resolution,
        bool forTooltip)
    {
        if (resolution == null || !resolution.IsPuppy)
            yield break;

        if (resolution.TryGetPairBonus(out PuppyPairBonusDefinition pairBonus))
        {
            string pairBonusText = GetPairBonusText(resolution, pairBonus, forTooltip);
            if (!string.IsNullOrEmpty(pairBonusText))
            {
                yield return pairBonusText;
                yield break;
            }
        }

        yield return GetBarkText();
    }

    public static string GetBarkText()
    {
        string directionKey = Main.ReversedUpDownArmorSetBonuses
            ? BarkDirectionUpLocalizationKey
            : BarkDirectionDownLocalizationKey;
        string direction = Language.GetTextValue(directionKey);
        return Language.GetTextValue(BarkLocalizationKey, direction);
    }

    public static string GetPairBonusText(
        PuppyEquipmentResolution resolution,
        PuppyPairBonusDefinition pairBonus,
        bool forTooltip)
    {
        if (pairBonus == null)
            return string.Empty;

        // Ore sight text follows the pair's placement state (Costume/Furry/Therian).
        if (pairBonus.Spelunker.HasValue)
            return Language.GetTextValue(GetSpelunkerLocalizationKey(resolution.SelectedPlacement));

        string localizationKey = forTooltip
            ? pairBonus.TooltipLocalizationKey ?? pairBonus.SetBonusLocalizationKey
            : pairBonus.SetBonusLocalizationKey;
        if (string.IsNullOrEmpty(localizationKey))
            return string.Empty;

        // Defense text shows the value for the pair's placement state (Costume/Furry/Therian).
        if (pairBonus.PlacementDefense.HasValue)
            return Language.GetTextValue(localizationKey, pairBonus.PlacementDefense.Value.GetDefense(resolution.SelectedPlacement));

        return Language.GetTextValue(localizationKey);
    }

    private static string GetSpelunkerLocalizationKey(PuppySetPlacement placement) => placement switch
    {
        PuppySetPlacement.Therian => SpelunkerTherianLocalizationKey,
        PuppySetPlacement.Furry => SpelunkerFurryLocalizationKey,
        _ => SpelunkerCostumeLocalizationKey
    };
}
