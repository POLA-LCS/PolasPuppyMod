using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Tooltip;
using static PuppyMod.Common.Tooltip.TooltipExtensions;

namespace PuppyMod.Content.Items.Collar;

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

        int index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Tooltip0");
        if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
        if (index == -1) index = tooltips.Count - 1;

        tooltips.Insert(index + 1, new TooltipLine(Mod, "AttachedLabel", LabelColor("Attached:", ColorAttachedLabel)));
        tooltips.Insert(index + 2, new TooltipLine(Mod, "CollarOwnerDefense", $"{LabelColor("Owner:", ColorOwnerLabel)} +2 defense"));

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
