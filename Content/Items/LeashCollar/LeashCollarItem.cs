using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.LeashCollar;

public class LeashCollarItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.accessory = true;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(gold: 1, silver: 4);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<ChainedPlayer>().hasChainLeash = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Chain, 2)
            .AddIngredient(ItemID.Leather, 10)
            .AddTile(TileID.Loom)
            .Register();
    }
}
