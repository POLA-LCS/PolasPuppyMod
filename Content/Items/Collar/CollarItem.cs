using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Collar;
public class CollarItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.accessory = true;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(silver: 27, copper: 1);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        // good puppy deserves a shiny collar :3
        player.GetModPlayer<ChainedPlayer>().hasCollar = true;
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