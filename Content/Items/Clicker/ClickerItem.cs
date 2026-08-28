using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.Items.Clicker;

public class ClickerItem : BaseClickerItem
{

    public override void SetDefaults()
    {
        base.SetDefaults();
        TileRange = 10;
        UsageCooldown = 60; // 1 second cooldown between clicks
        BuffDuration = 180; // 3 seconds Good Puppy buff
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 1, silver: 4, copper: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.IronBar, 5)
            .AddIngredient(ItemID.PinkPricklyPear, 1)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe(1)
            .AddIngredient(ItemID.LeadBar, 5)
            .AddIngredient(ItemID.PinkPricklyPear, 1)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe(1)
            .AddIngredient(ItemID.IronBar, 5)
            .AddIngredient(ItemID.PinkGel, 10)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();

        CreateRecipe(1)
            .AddIngredient(ItemID.LeadBar, 5)
            .AddIngredient(ItemID.PinkGel, 10)
            .AddIngredient(ItemID.Chain, 3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
