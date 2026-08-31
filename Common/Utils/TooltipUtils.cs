using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace PuppyMod.Common.Utils;

public static class TooltipUtils
{
    public static void AddLeashTooltip(List<TooltipLine> tooltips, Mod mod, int tiles)
    {
        var rangeTooltip = new TooltipLine(mod, "LeashRange", $"{tiles} leash range") { OverrideColor = new Color(193, 154, 107) };
        int dmgIdx = tooltips.FindIndex(l => l.Name == "Damage");
        if (dmgIdx >= 0)
        {
            tooltips.Insert(dmgIdx + 1, rangeTooltip);
            return;
        }
        int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        if (priceIdx >= 0)
        {
            tooltips.Insert(priceIdx, rangeTooltip);
        }
        else
        {
            tooltips.Add(rangeTooltip);
        }
    }

    public static void AddPenaltyTooltip(List<TooltipLine> tooltips, Mod mod, string penalty)
    {
        var line = new TooltipLine(mod, "LeashPenalty", penalty) { OverrideColor = Color.LightGray };
        int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        if (priceIdx >= 0)
        {
            tooltips.Insert(priceIdx, line);
        }
        else
        {
            tooltips.Add(line);
        }
    }

    public static void InsertRangeBeforePrice(List<TooltipLine> tooltips, IEnumerable<TooltipLine> lines)
    {
        int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        if (priceIdx >= 0)
        {
            tooltips.InsertRange(priceIdx, lines);
            return;
        }
        int kbIdx = tooltips.FindIndex(l => l.Name == "Knockback");
        if (kbIdx >= 0)
        {
            tooltips.InsertRange(kbIdx + 1, lines);
            return;
        }
        tooltips.AddRange(lines);
    }
}
