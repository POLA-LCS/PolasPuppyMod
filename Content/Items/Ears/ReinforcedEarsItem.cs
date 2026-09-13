using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;

namespace PuppyMod.Content.Items.Ears;

[AutoloadEquip(EquipType.Head)]
public class ReinforcedEarsItem : ModItem, IPuppyEars
{
    public PuppyEquipmentStats Stats => new(
        Defense: 2f,
        MeleeKnockbackAdditive: 0.25f,
        SummonKnockbackFlat: 0.25f);

    public override string Texture => "PuppyMod/Assets/Armor/ReinforcedDogEarsArmor";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogEars);
        // CloneDefaults overwrites the autoloaded head slot with vanilla DogEars (242), which would render vanilla ears; restore the mod equip slot.
        Item.headSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        // Defense via central stats; see Puppy-Set-System.md
        Item.defense = 0;
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
