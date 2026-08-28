using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.Items.GoldenClicker;

public class GoldenClickerItem : BaseClickerItem
{

    public override void SetDefaults()
    {
        base.SetDefaults();
        TileRange = 15;
        UsageCooldown = 45; // slightly faster than regular
        BuffDuration = 240; // 4 seconds
        Item.rare = ItemRarityID.Pink;
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
