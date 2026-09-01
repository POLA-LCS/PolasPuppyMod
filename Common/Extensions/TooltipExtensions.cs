using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;

namespace PuppyMod.Common.Extensions;

public static class TooltipExtensions
{
    public static void InsertLines(this List<TooltipLine> tooltips, IEnumerable<TooltipLine> lines)
    {
        var list = lines as ICollection<TooltipLine> ?? new List<TooltipLine>(lines);
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

    public static void AddRangeTooltip(this List<TooltipLine> tooltips, Mod mod, string lineName, int rangeTiles, string suffix, Color color)
        => AddRangeTooltipCore(tooltips, mod, lineName, rangeTiles, suffix, color);
}
