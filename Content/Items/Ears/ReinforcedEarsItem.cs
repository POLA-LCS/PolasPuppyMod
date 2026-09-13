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
        MeleeKnockbackAdditive: 0.25f,
        SummonKnockbackFlat: 0.25f);

    public override string Texture => "Terraria/Images/Item_" + ItemID.DogEars;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogEars);
        // Defense handled via PuppyEquipmentStats / Player.statDefense (PostUpdateEquips) so vanity correctly gives halved value (functional 2 / vanity 1).
        // Item.defense is not used because vanity slots ignore Item.defense (0) and using it would duplicate with stats; tooltip "2 defense" is custom localization.
        // Knockback/movement/jump bonuses are applied via Player fields (Player.GetKnockback etc.), not Item fields, per tModLoader best practice.
        Item.defense = 0;
        Item.accessory = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.DogEars, 1)
            .AddRecipeGroup(RecipeGroupID.IronBar, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, Item);
}
