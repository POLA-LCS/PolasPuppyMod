using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;

namespace PuppyMod.Content.Items.Tail;

public class ReinforcedTailItem : ModItem, IPuppyTailItem
{
    public PuppyEquipmentStats Stats => new(
        Defense: 2f,
        MoveSpeed: 0.20f,
        AccRunSpeed: 0.30f,
        MaxRunSpeed: 0.20f,
        JumpSpeedBoost: 0.6666667f);

    public override string Texture => "PuppyMod/Assets/Armor/ReinforcedDogTailArmor";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogTail);
        // CloneDefaults resets backSlot to vanilla DogTail (25); restore the animated equip slot.
        Item.backSlot = EquipLoader.GetEquipSlot(Mod, PuppyEquipmentTextures.ReinforcedTailBack, EquipType.Back);
        // Defense via central stats; see Puppy-Set-System.md
        Item.defense = 0;
        Item.accessory = true;
    }

    public override void SetStaticDefaults()
    {
        // Draw the tail in the player's tail layer instead of the backpack layer.
        ArmorIDs.Back.Sets.DrawInTailLayer[Item.backSlot] = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.DogTail, 1)
            .AddRecipeGroup(RecipeGroupID.IronBar, 20)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, Item);
}
