using System.Collections.Generic;
using PuppyMod.Common.Tooltip;
using Terraria;
using Terraria.ModLoader;

namespace PuppyMod.Content.GlobalItems
{
    public abstract class PuppyGlobalItem : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
            => tooltips.ApplyPuppyEquipmentTooltip(Mod, item);
    }
}
