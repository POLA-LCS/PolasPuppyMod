using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Tooltip;
using PuppyMod.Common.Utils;
using PuppyMod.Players;

namespace PuppyMod.Content.Items.Collars;

public abstract class BaseCollarItem : ModItem, ICollarItem, ITooltipProvider
{
    public override string Texture => AssetUtils.GetAccessoryTexturePathWithFallback(Name);

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.accessory = true;
        Item.maxStack = 1;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<ChainedPlayer>().SetCollarActive(Type);
        AffectWearer(player);
    }

    protected virtual void AffectWearer(Player player) { }

    public virtual void AffectOwner(Player owner) { }

    public virtual IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        yield break;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => tooltips.ApplyTooltips(Mod, this);
}
