using System.Collections.Generic;
using PuppyMod.Common.Tooltip;
using Terraria;
using Terraria.ModLoader;

namespace PuppyMod.Content.GlobalItems;

// H7: Two subclasses (DogEarsGlobalItem/DogTailGlobalItem) inherit this with same ModifyTooltips forwarding.
// Dual is intentional filter per tModLoader best practice – separate AppliesToEntity per item type for caching and clarity,
// rather than single PuppyGlobalItem with type==DogEars||DogTail check. Keeps filter narrow and avoids global overhead.
// Alternative merge would be single concrete PuppyGlobalItem with AppliesToEntity => type==DogEars||DogTail.
public abstract class PuppyGlobalItem : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, item);
}
