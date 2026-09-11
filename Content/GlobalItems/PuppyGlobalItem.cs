using System.Collections.Generic;
using System.Linq;
using PuppyMod.Common.Tooltip;
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
            tooltips.StripVanity();
            tooltips.ApplyPuppyFlavor(Mod);

            bool hasAcc = HasAccessory, hasVan = HasVanity;

            int index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Tooltip0");
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
            if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Defense");
            if (index == -1) index = tooltips.Count - 1;

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

            int bonusIdx = tooltips.FindIndex(l => l.Name == "Puppy" + EarOrTailName + "Flavor");
            if (bonusIdx == -1) bonusIdx = tooltips.FindIndex(l => l.Name == "Puppy" + EarOrTailName + "Half");
            tooltips.InsertPuppyBonus(Mod, HasAccessory || HasVanity, bonusIdx + 1);
            tooltips.MovePriceToBottom();
        }
    }
}
