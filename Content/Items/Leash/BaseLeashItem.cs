using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Constants;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Utils;
using PuppyMod.Players;
using PuppyMod.Services.Leash;

namespace PuppyMod.Content.Items.Leash;

public abstract class BaseLeashItem : ModItem, ILeashItem, IRangeUsable
{
    public abstract int LeashRangeTiles { get; }
    public int RangeTiles => LeashRangeTiles;
    public virtual string RopeTexturePath => PuppyConstants.RopeTexturePath;
    public virtual Color RopeColor => PuppyConstants.RopeColor;
    protected abstract DamageClass LeashDamageClass { get; }
    protected virtual int BaseDamage => 18;
    protected virtual float BaseKnockback => 3f;

    private int _origStyle;
    private int _origTime;
    private int _origAnim;
    private bool _hasOriginal;

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
        Item.DamageType = LeashDamageClass;
        Item.damage = BaseDamage;
        Item.knockBack = BaseKnockback;
        Item.value = Item.sellPrice(silver: 50);
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item1;
    }

    public override bool AltFunctionUse(Player player) => true;

    private void EnsureOriginalCached()
    {
        if (_hasOriginal) return;
        _origStyle = Item.useStyle;
        _origTime = Item.useTime;
        _origAnim = Item.useAnimation;
        _hasOriginal = true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.GetModPlayer<PuppyPlayer>().IsPuppy)
            return false;

        if (player.altFunctionUse != 2)
            EnsureOriginalCached();

        if (player.altFunctionUse == 2)
        {
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.useTime = 12;
            Item.useAnimation = 12;
        }
        else if (_hasOriginal)
        {
            Item.useStyle = _origStyle;
            Item.useTime = _origTime;
            Item.useAnimation = _origAnim;
        }

        if (LeashService.IsLeashing(player, Type))
        {
            Item.useTime = (int)(Item.useTime * PuppyConstants.LeashPenaltyUseTimeMult);
            Item.useAnimation = (int)(Item.useAnimation * PuppyConstants.LeashPenaltyUseTimeMult);
            Item.damage = (int)(BaseDamage * PuppyConstants.LeashPenaltyDamageMult);
            Item.knockBack = BaseKnockback * PuppyConstants.LeashPenaltyKnockMult;
        }
        else
        {
            Item.damage = BaseDamage;
            Item.knockBack = BaseKnockback;
        }
        return base.CanUseItem(player);
    }

    public override bool CanShoot(Player player) => player.altFunctionUse != 2;

    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            var target = LeashService.FindPuppyUnderCursor(player, LeashRangeTiles);
            if (target == null) return false;
            var chain = target.GetModPlayer<ChainedPlayer>();
            bool ownedByMe = chain.GrabberIndex == player.whoAmI;
            if (ownedByMe)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    ModContent.GetInstance<PuppyMod>().RequestLeashDetach(target.whoAmI);
                else
                    chain.SetGrabberAuthority(-1, 0);
            }
            else
            {
                if (chain.GrabberIndex.HasValue && chain.GrabberIndex != player.whoAmI)
                    return false;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    ModContent.GetInstance<PuppyMod>().RequestLeashAttach(target.whoAmI, Type);
                else
                    chain.SetGrabberAuthority(player.whoAmI, Type);
            }
            return true;
        }
        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        TooltipUtils.AddLeashRange(tooltips, Mod, LeashRangeTiles);
        TooltipUtils.AddPenalty(tooltips, Mod);
        if (LeashExtraDescription != null)
        {
            var extra = new TooltipLine(Mod, "LeashExtra", LeashExtraDescription) { OverrideColor = Microsoft.Xna.Framework.Color.LightGreen };
            tooltips.Add(extra);
        }
    }

    public virtual void AffectPuppy(Player player)
    {
        return;
    }

    protected virtual string LeashExtraDescription => null;
}
