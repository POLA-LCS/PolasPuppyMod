using System.Collections.Generic;
using System.Linq;
using PuppyMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.GlobalItems
{
    public class DogTailGlobalItem : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogTail;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            puppy.HasDogTailAccessory = true;
        }

        public override void UpdateEquip(Item item, Player player)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            puppy.HasDogTailAccessory = true;

        }

        public override void UpdateVanity(Item item, Player player)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            puppy.HasDogTailVanity = true;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // Puppy is puppy no matter what Terraria says!
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "Social");
            tooltips.RemoveAll(l => l.Mod == "Terraria" && l.Name == "SocialDesc");
            tooltips.RemoveAll(l => l.Text.Contains("No stats will be gained") || l.Text.Contains("Equipped in social slot"));

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

            bool tailAcc = false, tailVan = false;
            if (Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                var helper = Main.LocalPlayer.GetModPlayer<PolasBasePlayer>();
                tailAcc = helper.HasInAccessory(ItemID.DogTail);
                tailVan = helper.HasInVanity(ItemID.DogTail);
            }

            int index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Tooltip0");
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Defense");
            if (index == -1) index = tooltips.Count - 1;

            // *wag wag* zoomies time! :3
            if (tailAcc)
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyTailStat", "Increase mobility"));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyTailFlavor", "'Walkies = zoomies! Aarr-woof! :3'"));
            }
            else if (tailVan)
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyTailHalf", "[c/C8C864:halved]: Increase mobility"));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyTailFlavor", "'Walkies = zoomies! Aarr-woof! :3'"));
            }
            else
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyTail", "Increase mobility"));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyTailFlavor", "'Walkies = zoomies! Aarr-woof! :3'"));
            }

            if (Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                var puppy = Main.LocalPlayer.GetModPlayer<PuppyPlayer>();
                if (puppy.IsPuppy)
                {
                    var bonusLine = new TooltipLine(Mod, "PuppyBonus", "Puppy bonus: Double tap to bark! Arf! Woof! :3") { OverrideColor = new Microsoft.Xna.Framework.Color(255, 190, 125) };
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
