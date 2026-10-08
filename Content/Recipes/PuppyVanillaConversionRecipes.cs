using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Recipes;

/// <summary>
/// Cross-crafting between the vanilla cosmetic pairs. Dog Ears to Fox Ears, Fox Ears to Dog Ears,
/// Dog Tail to Fox Tail, and Fox Tail to Dog Tail, one for one. Both members of a pair count as a
/// full material for the other, so the conversion is symmetric and the result keeps the same Puppy
/// stats as the original item. No crafting station is required: the conversion is a plain swap.
/// </summary>
internal sealed class PuppyVanillaConversionRecipes : ModSystem
{
    public override void AddRecipes()
    {
        AddConversion(ItemID.FoxEars, ItemID.DogEars);
        AddConversion(ItemID.DogEars, ItemID.FoxEars);
        AddConversion(ItemID.FoxTail, ItemID.DogTail);
        AddConversion(ItemID.DogTail, ItemID.FoxTail);
    }

    private static void AddConversion(int resultType, int ingredientType)
    {
        Recipe.Create(resultType, 1)
            .AddIngredient(ingredientType, 1)
            .Register();
    }
}