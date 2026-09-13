using System.Collections.Generic;
using Terraria;
using Terraria.Localization;

namespace PuppyMod.Common.PuppySets;

public static class PuppySetBonusText
{
    public const string BarkLocalizationKey = "Mods.PuppyMod.SetBonuses.Bark";
    public const string BarkDirectionUpLocalizationKey = "Mods.PuppyMod.SetBonuses.BarkDirectionUp";
    public const string BarkDirectionDownLocalizationKey = "Mods.PuppyMod.SetBonuses.BarkDirectionDown";

    public static IEnumerable<string> GetActiveLines(
        PuppyEquipmentResolution resolution,
        bool forTooltip)
    {
        if (resolution == null || !resolution.IsPuppy)
            yield break;

        if (resolution.TryGetPairBonus(out PuppyPairBonusDefinition pairBonus))
        {
            string pairBonusText = GetPairBonusText(pairBonus, forTooltip);
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
        PuppyPairBonusDefinition pairBonus,
        bool forTooltip)
    {
        if (pairBonus == null)
            return string.Empty;

        string localizationKey = forTooltip
            ? pairBonus.TooltipLocalizationKey ?? pairBonus.SetBonusLocalizationKey
            : pairBonus.SetBonusLocalizationKey;
        return string.IsNullOrEmpty(localizationKey)
            ? string.Empty
            : Language.GetTextValue(localizationKey);
    }
}
