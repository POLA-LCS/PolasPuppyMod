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
        Item.defense = 2;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(silver: 27, copper: 1);
    }

    protected override void AffectWearer(Player player)
    {
        player.statDefense += 1;
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
