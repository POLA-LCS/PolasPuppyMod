using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Physics;
using PuppyMod.Common.Tooltip;
using PuppyMod.Services.Leash;

namespace PuppyMod.Content.Items.Leash;

public abstract class BaseUtilityLeashItem : ModItem, ILeashItem, ITooltipProvider
{
    public abstract int RangeTiles { get; }
    public abstract string LeashTexturePath { get; }
    public virtual LeashPhysicsProfile Physics => new();

    public override bool CanShoot(Player player) => false;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 22;
        Item.useAnimation = 22;
        Item.useTurn = true;
        Item.autoReuse = false;
        Item.maxStack = 1;
        Item.damage = 0;
        Item.knockBack = 0f;
        Item.value = Item.sellPrice(silver: 50);
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ProjectileID.None;
        Item.shootSpeed = 0f;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        if (player.GetModPlayer<Players.PuppyPlayer>().IsPuppy)
            return false;
        return base.CanUseItem(player);
    }

    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            bool toggled = LeashAttachService.TryToggleLeash(player, Type, RangeTiles);
            return toggled;
        }
        return base.UseItem(player);
    }

    public virtual void AffectPuppy(Player puppy) { }

    public virtual IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        yield return new TooltipLine(mod, "LeashRange", $"{RangeTiles} leash range") { OverrideColor = new Color(165, 150, 135) };
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => tooltips.ApplyTooltips(Mod, this);
}
