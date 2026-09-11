using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using PuppyMod.Players;

namespace PuppyMod.Common.Tooltip;

public static class TooltipExtensions
{
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
        tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "Social");
        tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "SocialDesc");
        tooltips.RemoveAll(l => l.Text.Contains("No stats will be gained") || l.Text.Contains("Equipped in social slot"));
        tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "SetBonus");
        tooltips.RemoveAll(l => l.Text.Contains("Set bonus:"));
        tooltips.RemoveAll(l => l.Text == "Vanity Item" || l.Text.Contains("Vanity Item"));
    }

    public static void InsertPuppyBonus(this List<TooltipLine> tooltips, Mod mod, bool isEquipped, int insertIndex)
    {
        if (!isEquipped) return;
        if (tooltips.Any(l => l.Name == "PuppyBonus")) return;
        if (Main.LocalPlayer == null || !Main.LocalPlayer.active) return;

        var puppy = Main.LocalPlayer.GetModPlayer<Players.PuppyPlayer>();
        if (!puppy.IsPuppy) return;

        string dir = Main.ReversedUpDownArmorSetBonuses ? "UP" : "DOWN";
        var bonusLine = new TooltipLine(mod, "PuppyBonus", $"Puppy bonus: Double tap {dir} to bark, arf!") { OverrideColor = new Color(255, 190, 125) };
        int idx = insertIndex >= 0 && insertIndex <= tooltips.Count ? insertIndex : tooltips.Count;
        tooltips.Insert(idx, bonusLine);
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
        if (equipableLines.Count > 0)
        {
            equipableLines[0].Text = "Vanity/Equipable";
            for (int i = 1; i < equipableLines.Count; i++)
                tooltips.Remove(equipableLines[i]);
        }
        else
        {
            int insertIdx = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "ItemName");
            if (insertIdx == -1) insertIdx = 0;
            tooltips.Insert(insertIdx + 1, new TooltipLine(mod, "PuppyVanityEquipable", "Vanity/Equipable"));
        }

        bool hasVanityItem = tooltips.Any(l => l.Text == "Vanity Item" || l.Text.Contains("Vanity Item"));
        if (hasVanityItem)
        {
            foreach (var line in tooltips)
            {
                if (line.Text == "Vanity Item" || line.Text.Contains("Vanity Item"))
                {
                    line.Text = "Release the puppiness!";
                    line.OverrideColor = new Color(193, 154, 107);
                    break;
                }
            }
        }
        else
        {
            bool hasRelease = tooltips.Any(l => l.Text == "Release the puppiness!");
            if (!hasRelease)
            {
                int idx = tooltips.FindIndex(l => l.Text == "Vanity/Equipable");
                if (idx != -1)
                    tooltips.Insert(idx + 1, new TooltipLine(mod, "PuppyRelease", "Release the puppiness!") { OverrideColor = new Color(193, 154, 107) });
                else
                    tooltips.Insert(1, new TooltipLine(mod, "PuppyRelease", "Release the puppiness!") { OverrideColor = new Color(193, 154, 107) });
            }
        }
    }
}
