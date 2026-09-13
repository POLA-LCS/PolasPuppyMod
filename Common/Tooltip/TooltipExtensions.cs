using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;

namespace PuppyMod.Common.Tooltip;

public static class TooltipExtensions
{
    private const string PuppyVanityEquipableLineName = "PuppyVanityEquipable";
    private const string PuppyReleaseLineName = "PuppyRelease";
    private const string PuppyBarkBonusLineName = "PuppyBarkBonus";
    private const string PuppyPairBonusLineName = "PuppyPairBonus";
    private const string PuppyVanityEquipableLocalizationKey = "Mods.PuppyMod.Tooltips.VanityEquipable";
    private const string PuppyReleaseLocalizationKey = "Mods.PuppyMod.Tooltips.Release";
    private const string PuppyHalvedLocalizationKey = "Mods.PuppyMod.Tooltips.Halved";

    public static readonly Color ColorPuppyLabel = new(193, 154, 107);
    public static readonly Color ColorOwnerLabel = new(120, 176, 56);
    public static readonly Color ColorAttachedLabel = new(255, 180, 225);
    public static readonly Color ColorLeashRange = new(170, 170, 170);
    public static readonly Color ColorPuppyBonus = new(255, 190, 125);
    public static readonly Color ColorHalved = new(200, 200, 100);

    public static string LabelColor(string text, Color color)
    {
        int rgb = (color.R << 16) | (color.G << 8) | color.B;
        return $"[c/{rgb:X6}:{text}]";
    }
    public static void InsertLines(this List<TooltipLine> tooltips, IEnumerable<TooltipLine> lines)
    {
        var list = lines as ICollection<TooltipLine> ?? [.. lines];
        if (list.Count == 0)
            return;
        int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        if (priceIdx >= 0)
        {
            tooltips.InsertRange(priceIdx, list);
            return;
        }
        int kbIdx = tooltips.FindIndex(l => l.Name == "Knockback");
        if (kbIdx >= 0)
        {
            tooltips.InsertRange(kbIdx + 1, list);
            return;
        }
        tooltips.AddRange(list);
    }

    public static void ApplyTooltips(this List<TooltipLine> tooltips, Mod mod, ITooltipProvider provider)
        => tooltips.InsertLines(provider.GetTooltipLines(mod));

    public static void AddRangeTooltip(this List<TooltipLine> tooltips, Mod mod, string lineName, int rangeTiles, string suffix, Color color)
        => AddRangeTooltipCore(tooltips, mod, lineName, rangeTiles, suffix, color);

    private static void AddRangeTooltipCore(List<TooltipLine> tooltips, Mod mod, string lineName, int rangeTiles, string suffix, Color color)
    {
        var line = new TooltipLine(mod, lineName, $"{rangeTiles} {suffix}") { OverrideColor = color };
        int dmgIdx = tooltips.FindIndex(l => l.Name == "Damage");
        if (dmgIdx >= 0)
        {
            tooltips.Insert(dmgIdx + 1, line);
            return;
        }
        int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        if (priceIdx >= 0)
            tooltips.Insert(priceIdx, line);
        else
            tooltips.Add(line);
    }

    public static void StripVanity(this List<TooltipLine> tooltips)
    {
        tooltips.RemoveAll(l => l.Mod == "Terraria" &&
            (l.Name == "Social" || l.Name == "SocialDesc" || l.Name == "SetBonus"));
    }

    public static void ApplyPuppyEquipmentTooltip(
        this List<TooltipLine> tooltips,
        Mod mod,
        Item item)
    {
        if (item == null || !PuppyEquipmentRegistry.TryGetDefinition(item.type, out PuppyEquipmentDefinition definition))
            return;
        if (definition.Tooltip == null || definition.Tooltip.Lines.Count == 0)
            return;

        tooltips.StripVanity();
        tooltips.ApplyPuppyFlavor(mod);

        bool hasFunctional = false;
        bool hasVanity = false;
        PuppyPlayer puppy = null;
        if (!Main.dedServ && Main.LocalPlayer != null && Main.LocalPlayer.active)
        {
            puppy = Main.LocalPlayer.GetModPlayer<PuppyPlayer>();
            hasFunctional = puppy.EquipmentSnapshot.ContainsFunctional(item.type);
            hasVanity = puppy.EquipmentSnapshot.ContainsVanity(item.type);
        }

        int anchorIndex = FindTooltipAnchor(tooltips);
        int insertIndex = anchorIndex >= 0 ? anchorIndex + 1 : tooltips.Count;
        bool vanityOnly = hasVanity && !hasFunctional;
        foreach (PuppyTooltipLineDefinition lineDefinition in definition.Tooltip.Lines)
        {
            string text = Language.GetTextValue(lineDefinition.LocalizationKey);
            if (lineDefinition.HalveInVanity && vanityOnly)
                text = Language.GetTextValue(PuppyHalvedLocalizationKey, text);

            tooltips.Insert(insertIndex++, new TooltipLine(mod, lineDefinition.LineName, text));
        }

        if (puppy != null && (hasFunctional || hasVanity))
        {
            bool isBarkLine = true;
            foreach (string bonusText in PuppySetBonusText.GetActiveLines(puppy.EquipmentResolution, forTooltip: true))
            {
                string lineName = isBarkLine ? PuppyBarkBonusLineName : PuppyPairBonusLineName;
                isBarkLine = false;
                if (tooltips.Any(line => line.Name == lineName))
                    continue;

                tooltips.Insert(
                    insertIndex++,
                    new TooltipLine(mod, lineName, bonusText) { OverrideColor = ColorPuppyBonus });
            }
        }

        tooltips.MovePriceToBottom();
    }

    public static void MovePriceToBottom(this List<TooltipLine> tooltips)
    {
        int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        if (priceIdx < 0) return;
        var price = tooltips[priceIdx];
        tooltips.RemoveAt(priceIdx);
        tooltips.Add(price);
    }

    public static void ApplyPuppyFlavor(this List<TooltipLine> tooltips, Mod mod)
    {
        var equipableLines = tooltips.Where(l => l.Mod == "Terraria" && (l.Name == "Equipable" || l.Name == "Vanity")).ToList();
        int mergedIndex;
        if (equipableLines.Count > 0)
        {
            mergedIndex = tooltips.FindIndex(l => l.Mod == "Terraria" && (l.Name == "Equipable" || l.Name == "Vanity"));
            equipableLines[0].Text = Language.GetTextValue(PuppyVanityEquipableLocalizationKey);
            for (int i = 1; i < equipableLines.Count; i++)
                tooltips.Remove(equipableLines[i]);
        }
        else
        {
            mergedIndex = tooltips.FindIndex(l => l.Name == PuppyVanityEquipableLineName);
            if (mergedIndex == -1)
            {
                int insertIdx = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "ItemName");
                if (insertIdx == -1) insertIdx = 0;
                mergedIndex = insertIdx + 1;
                tooltips.Insert(
                    mergedIndex,
                    new TooltipLine(mod, PuppyVanityEquipableLineName, Language.GetTextValue(PuppyVanityEquipableLocalizationKey)));
            }
        }

        bool hasRelease = tooltips.Any(l => l.Name == PuppyReleaseLineName);
        if (!hasRelease)
        {
            tooltips.Insert(
                mergedIndex + 1,
                new TooltipLine(mod, PuppyReleaseLineName, Language.GetTextValue(PuppyReleaseLocalizationKey))
                {
                    OverrideColor = ColorPuppyLabel
                });
        }
    }

    private static int FindTooltipAnchor(List<TooltipLine> tooltips)
    {
        int index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Tooltip0");
        if (index == -1)
            index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip", StringComparison.Ordinal));
        if (index == -1)
            index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Defense");
        return index;
    }
}
