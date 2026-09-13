using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using PuppyMod.Common.Tooltip;
using static PuppyMod.Common.Tooltip.TooltipExtensions;

namespace PuppyMod.Content.Items.Collars;

public class CollarItem : BaseCollarItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        // Defense comes from statDefense; Item.defense stays 0 to avoid double counting.
        Item.defense = 0;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(silver: 27, copper: 1);
    }

    protected override void AffectWearer(Player player)
    {
        // Functional wearer bonus only; vanity collars give no defense.
        player.statDefense += 2;
    }

    public override void AffectOwner(Player owner)
    {
        owner.statDefense += 2;
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        yield break;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.StripVanity();

        int anchor = TooltipExtensions.FindTooltipAnchor(tooltips);
        int index = anchor >= 0 ? anchor + 1 : tooltips.Count;
        tooltips.Insert(index, new TooltipLine(Mod, "AttachedLabel", ColorizeLabel(Language.GetTextValue("Mods.PuppyMod.Tooltips.Attached"), ColorAttachedLabel)));
        tooltips.Insert(index + 1, new TooltipLine(Mod, "CollarOwnerDefense", $"{ColorizeLabel(Language.GetTextValue("Mods.PuppyMod.Tooltips.Owner"), ColorOwnerLabel)} +2 defense"));

        tooltips.MovePriceToBottom();
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Silk, 10)
            .AddRecipeGroup(RecipeGroupID.IronBar, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
