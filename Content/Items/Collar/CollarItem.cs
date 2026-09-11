using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Collar;

public class CollarItem : BaseCollarItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.defense = 2;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(silver: 27, copper: 1);
    }

    protected override void AffectWearer(Player player)
    {
        player.statDefense += 1;
    }

    public override void AffectOwner(Player owner)
    {
        owner.statDefense += 2;
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        yield break;
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Silk, 10)
            .AddRecipeGroup(RecipeGroupID.IronBar, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
