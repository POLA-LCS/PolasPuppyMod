using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;

namespace PuppyMod.Content.Items.Tail;

public class ReinforcedTailItem : ModItem, IPuppyTail
{
    public PuppyEquipmentStats Stats => new(
        Defense: 2f,
        MoveSpeed: 0.20f,
        AccRunSpeed: 0.30f,
        MaxRunSpeed: 0.20f,
        JumpSpeedBoost: 0.6666667f);

    public override string Texture => "Terraria/Images/Item_" + ItemID.DogTail;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogTail);
        // Defense via central stats; see Puppy-Set-System.md
        Item.defense = 0;
        Item.accessory = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.DogTail, 1)
            .AddRecipeGroup(RecipeGroupID.IronBar, 20)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, Item);
}
