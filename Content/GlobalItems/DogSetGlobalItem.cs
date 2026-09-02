using System.Collections.Generic;
using System.Linq;
using PuppyMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.GlobalItems
{
    public class DogSetGlobalItem : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogEars || entity.type == ItemID.DogTail;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            if (item.type == ItemID.DogEars) puppy.HasDogEarsAccessory = true;
            if (item.type == ItemID.DogTail) puppy.HasDogTailAccessory = true;
        }

        public override void UpdateEquip(Item item, Player player)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            if (item.type == ItemID.DogEars) puppy.HasDogEarsAccessory = true;
            if (item.type == ItemID.DogTail) puppy.HasDogTailAccessory = true;
        }

        public override void UpdateVanity(Item item, Player player)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            if (item.type == ItemID.DogEars) puppy.HasDogEarsVanity = true;
            if (item.type == ItemID.DogTail) puppy.HasDogTailVanity = true;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type != ItemID.DogEars && item.type != ItemID.DogTail)
                return;

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

            bool earsAcc = false, earsVan = false, tailAcc = false, tailVan = false;
            if (Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                var helper = Main.LocalPlayer.GetModPlayer<PolasBasePlayer>();
                earsAcc = helper.HasInAccessory(ItemID.DogEars);
                earsVan = helper.HasInVanity(ItemID.DogEars);
                tailAcc = helper.HasInAccessory(ItemID.DogTail);
                tailVan = helper.HasInVanity(ItemID.DogTail);
            }

            int index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Tooltip0");
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Defense");
            if (index == -1) index = tooltips.Count - 1;

            // Don't judge this statements, is going to make my life easier in a hipotetical future
            if (item.type == ItemID.DogEars)
            {
                if (earsAcc)
                {
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyEarsStat", "Increase digging speed"));
                    tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyEarsFlavor", "\u00b4Lets make a hole! *paw* *paw*\u00b4"));
                }
                else if (earsVan)
                {
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyEarsHalf", "Increase digging speed (halved)"));
                    tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyEarsFlavor", "\u00b4Lets make a hole! *paw* *paw*\u00b4"));
                }
                else
                {
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyEars", "Increase digging speed"));
                    tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyEarsFlavor", "\u00b4Lets make a hole! *paw* *paw*\u00b4"));
                }
            }
            else
            {
                if (tailAcc)
                {
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyTailStat", "Increase movility"));
                    tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyTailFlavor", "\u00b4Walkies will be zooming! aarrruf!\u00b4"));
                }
                else if (tailVan)
                {
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyTailHalf", "Increase movility (halved)"));
                    tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyTailFlavor", "\u00b4Walkies will be zooming! aarrruf!\u00b4"));
                }
                else
                {
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppyTail", "Increase movility"));
                    tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppyTailFlavor", "\u00b4Walkies will be zooming! aarrruf!\u00b4"));
                }
            }
        }
    }
}
