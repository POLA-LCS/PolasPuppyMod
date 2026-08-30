using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Collar;
public class CollarItem : ModItem
{
    public override void SetStaticDefaults()
    {
        // ItemID.Sets.WorksInVoidBag[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(silver: 27, copper: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Silk, 10)
            .AddIngredient(ItemID.IronBar, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}