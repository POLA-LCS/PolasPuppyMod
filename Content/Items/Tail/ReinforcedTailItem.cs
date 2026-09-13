using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;

namespace PuppyMod.Content.Items.Tail;

[AutoloadEquip(EquipType.Back)]
public class ReinforcedTailItem : ModItem, IPuppyTail
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
        // CloneDefaults overwrites the autoloaded back slot with vanilla DogTail (25), which would render the vanilla tail; restore the mod equip slot.
        Item.backSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Back);
        // Defense via central stats; see Puppy-Set-System.md
        Item.defense = 0;
        Item.accessory = true;
    }

    public override void SetStaticDefaults()
    {
        // Vanilla DogTail (25) draws in the tail layer; without this the modded back slot renders in the backpack layer (near the head).
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
