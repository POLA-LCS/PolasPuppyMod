using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace PuppyMod.Common.Utils;

public static class TooltipUtils
{
    public static void AddLeashRange(List<TooltipLine> tooltips, Mod mod, int tiles)
    {
        int idx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        var line = new TooltipLine(mod, "LeashRange", $"{tiles} leash range") { OverrideColor = new Color(193, 154, 107) };
        if (idx >= 0) tooltips.Insert(idx, line);
        else tooltips.Add(line);
    }

    public static void AddPenalty(List<TooltipLine> tooltips, Mod mod)
    {
        var line = new TooltipLine(mod, "LeashPenalty", "slower and weaker while walking a puppy") { OverrideColor = Color.Gray };
        tooltips.Add(line);
    }
}
