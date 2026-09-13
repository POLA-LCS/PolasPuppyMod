using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;
using PuppyMod.Players;

namespace PuppyMod.Content.Items.Tail;

public class ShinyTailItem : ModItem, IPuppyTail
{
    internal const int FunctionalHoverDuration = 30;
    internal const int VanityHoverDuration = 15;

    public PuppyEquipmentStats Stats => new(Defense: 0f);

    public override string Texture => "PuppyMod/Assets/Armor/ShinyDogTailArmor";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogTail);
        // Worn sprite is drawn manually by PuppyTailDrawLayer; clear the vanilla back slot so the vanilla tail isn't drawn.
        Item.backSlot = -1;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, Item);

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        ApplyHover(player, isVanity: false);
    }

    public override void UpdateVanity(Player player)
    {
        ApplyHover(player, isVanity: true);
    }

    public override void UpdateEquip(Player player)
    {
        ApplyHover(player, isVanity: false);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.DogTail, 1)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.Daybloom, 5)
            .AddIngredient(ItemID.GoldOre, 20)
            .AddTile(TileID.WorkBenches)
            .Register();

        // Alternative platinum variant – same cost, avoids forcing Gold world.
        CreateRecipe()
            .AddIngredient(ItemID.DogTail, 1)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.Daybloom, 5)
            .AddIngredient(ItemID.PlatinumOre, 20)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    private static void ApplyHover(Player player, bool isVanity)
    {
        player.carpet = true;
        player.GetModPlayer<PuppyPlayer>().EnableShinyTail(isVanity);
    }
}
