using System.Collections.Generic;
using System.Linq;
using PuppyMod.Common.Interfaces;
using PuppyMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace PuppyMod.Content.GlobalItems
{
    public abstract class PuppyGlobalItem : GlobalItem, ITooltipProvider
    {
        public abstract string EarOrTailName { get; }
        public abstract bool HasAccessory { get; }
        public abstract bool HasVanity { get; }
        public abstract string AccessoryTypeString { get; }
        public abstract string FlavorText { get; }

        public IEnumerable<TooltipLine> GetTooltipLines(Mod mod) => [];

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // Puppy is puppy no matter what Terraria says!
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "Social");
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "SocialDesc");
            tooltips.RemoveAll(l => l.Text.Contains("No stats will be gained") || l.Text.Contains("Equipped in social slot"));
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "SetBonus");
            tooltips.RemoveAll(l => l.Text.Contains("Set bonus:"));

            var equipableLines = tooltips.Where(l => l.Mod == "Terraria" && (l.Name == "Equipable" || l.Name == "Vanity")).ToList();
            if (equipableLines.Count > 0)
            {
                // Special Vanity/Equipable tooltip
                equipableLines[0].Text = "Vanity/Equipable";
                for (int i = 1; i < equipableLines.Count; i++)
                    tooltips.Remove(equipableLines[i]);
            }
            else // Just in case...
            {
                int insertIdx = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "ItemName");
                if (insertIdx == -1) insertIdx = 0;
                tooltips.Insert(insertIdx + 1, new TooltipLine(Mod, "PuppyVanityEquipable", "Vanity/Equipable"));
            }

            bool hasVanityItem = tooltips.Any(l => l.Text == "Vanity Item" || l.Text.Contains("Vanity Item"));
            if (hasVanityItem)
            {
                foreach (var line in tooltips)
                {
                    if (line.Text == "Vanity Item" || line.Text.Contains("Vanity Item"))
                    {
                        line.Text = "Release the puppiness!";
                        line.OverrideColor = new Microsoft.Xna.Framework.Color(193, 154, 107);
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
                        tooltips.Insert(idx + 1, new TooltipLine(Mod, "PuppyRelease", "Release the puppiness!") { OverrideColor = new Microsoft.Xna.Framework.Color(193, 154, 107) });
                    else
                        tooltips.Insert(1, new TooltipLine(Mod, "PuppyRelease", "Release the puppiness!") { OverrideColor = new Microsoft.Xna.Framework.Color(193, 154, 107) });
                }
            }

            bool hasAcc = HasAccessory, hasVan = HasVanity;

            int index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Tooltip0");
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Defense");
            if (index == -1) index = tooltips.Count - 1;

            // *paw paw* cute ears go zoom! :3 / *wag wag* zoomies time! :3
            if (hasAcc)
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "Puppy" + EarOrTailName + "Stat", AccessoryTypeString));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "Puppy" + EarOrTailName + "Flavor", FlavorText));
            }
            else if (hasVan)
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "Puppy" + EarOrTailName + "Half", "[c/C8C864:halved]: " + AccessoryTypeString));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "Puppy" + EarOrTailName + "Flavor", FlavorText));
            }
            else
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "Puppy" + EarOrTailName, AccessoryTypeString));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "Puppy" + EarOrTailName + "Flavor", FlavorText));
            }

            if (Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                var puppy = Main.LocalPlayer.GetModPlayer<PuppyPlayer>();
                if (puppy.IsPuppy)
                {
                    string dir = Main.ReversedUpDownArmorSetBonuses ? "UP" : "DOWN";
                    var bonusLine = new TooltipLine(Mod, "PuppyBonus", $"Puppy bonus: Double tap {dir} to bark, arf!") { OverrideColor = new Microsoft.Xna.Framework.Color(255, 190, 125) };
                    int bonusIdx = tooltips.FindLastIndex(x => x.Name.StartsWith("Tooltip"));
                    if (bonusIdx != -1)
                        tooltips.Insert(bonusIdx + 1, bonusLine);
                    else
                        tooltips.Add(bonusLine);
                }
            }
        }
    }
}
