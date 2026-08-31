using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Constants;
using PuppyMod.Common.Interfaces;
using PuppyMod.Players;
using PuppyMod.Services.Leash;

namespace PuppyMod.Content.Items.Leash;

public abstract class BaseLeashItem : ModItem, ILeashItem, IRangeUsable
{
    public abstract int LeashRangeTiles { get; }
    public int RangeTiles => LeashRangeTiles;
    protected abstract DamageClass LeashDamageClass { get; }
    protected virtual int BaseDamage => 18;
    protected virtual float BaseKnockback => 3f;

    private int ogStyle;
    private int ogTime;
    private int ogAnim;
    private bool ogCached;

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

    public override bool CanUseItem(Player player)
    {
        var puppyPlayer = player.GetModPlayer<PuppyPlayer>();
        if (puppyPlayer.IsPuppy)
            return false;

        if (!ogCached && player.altFunctionUse != 2)
        {
            ogStyle = Item.useStyle;
            ogTime = Item.useTime;
            ogAnim = Item.useAnimation;
            ogCached = true;
        }

        if (player.altFunctionUse == 2)
        {
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.useTime = 12;
            Item.useAnimation = 12;
        }
        else if (ogCached)
        {
            Item.useStyle = ogStyle;
            Item.useTime = ogTime;
            Item.useAnimation = ogAnim;
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
        int idx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        var rangeLine = new TooltipLine(Mod, "LeashRange", $"{LeashRangeTiles} leash range")
        {
            OverrideColor = new Color(193, 154, 107)
        };
        if (idx >= 0) tooltips.Insert(idx, rangeLine);
        else tooltips.Add(rangeLine);

        var penLine = new TooltipLine(Mod, "LeashPenalty", "slower and weaker while walking a puppy")
        {
            OverrideColor = Color.Gray
        };
        tooltips.Add(penLine);
    }

    public virtual void AffectPuppy(Player player)
    {
        return;
    }

    protected virtual string LeashExtraDescription => null;
}
