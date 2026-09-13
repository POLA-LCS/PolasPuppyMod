using Terraria;
using Terraria.ID;
using PuppyMod.Common.Utils;

namespace PuppyMod.Content.Items.Clickers;

public class ClickerItem : BaseClickerItem
{
    public override string Texture => AssetUtils.GetItemTexturePathWithFallback(nameof(ClickerItem));

    public override void SetDefaults()
    {
        base.SetDefaults();
        RangeTiles = 10;
        UsageCooldown = 60;
        BuffDuration = 180;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1, silver: 4, copper: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddRecipeGroup(RecipeGroupID.IronBar, 5)
            .AddIngredient(ItemID.PinkPricklyPear, 1)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe(1)
            .AddRecipeGroup(RecipeGroupID.IronBar, 5)
            .AddIngredient(ItemID.PinkGel, 10)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
