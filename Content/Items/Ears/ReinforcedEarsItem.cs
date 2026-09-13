using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;

namespace PuppyMod.Content.Items.Ears;

public class ReinforcedEarsItem : ModItem, IPuppyEarsItem
{
    public PuppyEquipmentStats Stats => new(
        Defense: 2f,
        MeleeKnockbackAdditive: 0.25f,
        SummonKnockbackFlat: 0.25f);

    public override string Texture => "PuppyMod/Assets/Armor/ReinforcedDogEarsArmor";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogEars);
        // CloneDefaults resets headSlot to vanilla DogEars (242); restore the animated equip slot.
        Item.headSlot = EquipLoader.GetEquipSlot(Mod, PuppyEquipmentTextures.ReinforcedEarsHead, EquipType.Head);
        // Defense via central stats; see Puppy-Set-System.md
        Item.defense = 0;
    }

    public override void SetStaticDefaults()
    {
        // Keep the player's hair visible under the ears.
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.DogEars, 1)
            .AddRecipeGroup(RecipeGroupID.IronBar, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, Item);
}
