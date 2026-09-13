using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;

namespace PuppyMod.Content.Items.Ears;

public class ReinforcedEarsItem : ModItem, IPuppyEars
{
    public PuppyEquipmentStats Stats => new(
        Defense: 2f,
        MeleeKnockbackAdditive: 0.5f,
        SummonKnockbackFlat: 0.5f);

    public override string Texture => "Terraria/Images/Item_" + ItemID.DogEars;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogEars);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, Item);
}
