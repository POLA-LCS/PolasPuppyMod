using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.GlobalItems
{
    public class DogSetGlobalItem : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.type != ItemID.DogEars && item.type != ItemID.DogTail)
                return;
            int index = tooltips.FindIndex(l => l.Name == "Tooltip0");
            if (index == -1) index = tooltips.Count - 1;
            if (item.type == ItemID.DogEars)
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppySet0", "Part of the Puppy set"));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppySet1", "Equip with Dog Tail to become a puppy"));
            }
            else
            {
                tooltips.Insert(index + 1, new TooltipLine(Mod, "PuppySet0", "Part of the Puppy set"));
                tooltips.Insert(index + 2, new TooltipLine(Mod, "PuppySet1", "Set bonus while both are equipped:"));
                tooltips.Insert(index + 3, new TooltipLine(Mod, "PuppySet2", "[c/FFB6C1:Improved mobility] and [c/FFD700:+15% mining speed]"));
                tooltips.Insert(index + 4, new TooltipLine(Mod, "PuppySet3", "Furry: +15% move speed, +0.3 jump"));
                tooltips.Insert(index + 5, new TooltipLine(Mod, "PuppySet4", "Therian (accessory slot): +30% move speed, +0.75 jump"));
                tooltips.Insert(index + 6, new TooltipLine(Mod, "PuppySet5", "Double tap UP to bark! Arf!"));
            }
        }
    }
}
