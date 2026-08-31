using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Utils;
using PuppyMod.Players;
using PuppyMod.Services.Leash;

namespace PuppyMod.Content.Items.Leash;

public abstract class BaseLeashItem : ModItem, ILeashItem, IWithRange
{
    public const float PenaltyUseTimeMult = 1.25f;
    public const float PenaltyDamageMult = 0.65f;
    public const float PenaltyKnockMult = 0.7f;

    public abstract int LeashRangeTiles { get; }
    public int RangeTiles => LeashRangeTiles;
    public abstract string LeashTexturePath { get; }
    public virtual float PuppyPull => 0.10f;
    public virtual float OwnerPull => 0.018f;
    protected abstract DamageClass LeashDamageClass { get; }
    protected virtual int BaseDamage => 18;
    protected virtual float BaseKnockback => 3f;

    private int originStyle;
    private int origTime;
    private int origAnim;
    private bool hasOriginal;

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
        if (hasOriginal) return;
        originStyle = Item.useStyle;
        origTime = Item.useTime;
        origAnim = Item.useAnimation;
        hasOriginal = true;
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
        else if (hasOriginal)
        {
            Item.useStyle = originStyle;
            Item.useTime = origTime;
            Item.useAnimation = origAnim;
        }

        if (LeashService.IsLeashing(player, Type))
        {
            Item.useTime = (int)(Item.useTime * PenaltyUseTimeMult);
            Item.useAnimation = (int)(Item.useAnimation * PenaltyUseTimeMult);
            Item.damage = (int)(BaseDamage * PenaltyDamageMult);
            Item.knockBack = BaseKnockback * PenaltyKnockMult;
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

    protected virtual IEnumerable<TooltipLine> GetOptionalTooltips()
    {
        yield return new TooltipLine(Mod, "LeashPenalty", "Weaker while puppy leashed") { OverrideColor = Color.LightGray };
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        TooltipUtils.AddLeashTooltip(tooltips, Mod, LeashRangeTiles);
        var optional = new List<TooltipLine>(GetOptionalTooltips());
        if (optional.Count == 0)
            return;
        int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        if (priceIdx >= 0)
        {
            tooltips.InsertRange(priceIdx, optional);
            return;
        }
        int kbIdx = tooltips.FindIndex(l => l.Name == "Knockback");
        if (kbIdx >= 0)
        {
            tooltips.InsertRange(kbIdx + 1, optional);
            return;
        }
        tooltips.AddRange(optional);
    }

    public virtual void AffectPuppy(Player player)
    {
        return;
    }
}
