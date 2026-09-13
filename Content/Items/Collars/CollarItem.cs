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
        // H1 fix: Align with Reinforced pattern – defense solely via statDefense with vanity halving.
        // Item.defense must be 0 to avoid double-count (previously 2 + statDefense 1 = 3 functional).
        // Vanity 0 defense is intentional: collar effect only via UpdateAccessory (functional) and ICollarItem for owner; vanity collar intentionally gives no wearer bonus.
        Item.defense = 0;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(silver: 27, copper: 1);
    }

    protected override void AffectWearer(Player player)
    {
        // Intent: +2 wearer defense functional, 0 vanity (documented above). Owner +2 via ICollarItem AffectOwner.
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

        // Unified anchor via TooltipExtensions.FindTooltipAnchor (Tooltip0 → Defense → Knockback/Price fallback).
        int anchor = TooltipExtensions.FindTooltipAnchor(tooltips);
        int index = anchor >= 0 ? anchor + 1 : tooltips.Count;
        // Insert via unified anchor before Price / after Knockback fallback already encoded in FindTooltipAnchor.
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
