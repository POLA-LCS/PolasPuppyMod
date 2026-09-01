using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.Items.Clicker;

public class GoldenClickerItem : BaseClickerItem
{

    public override void SetDefaults()
    {
        base.SetDefaults();
        RangeTiles = 15;
        UsageCooldown = 45;
        BuffDuration = 240; // 4 seconds
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(gold: 1, silver: 4, copper: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.IronBar, 5)
            .AddIngredient(ItemID.GoldBar, 10)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe(1)
            .AddIngredient(ItemID.LeadBar, 5)
            .AddIngredient(ItemID.GoldBar, 10)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
