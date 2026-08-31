using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using PuppyMod.Players;

namespace PuppyMod.Content.Items.Leash;

public abstract class BaseLeashItem : ModItem
{
    public abstract int LeashRangeTiles { get; }
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

    private bool IsLeashing(Player player)
    {
        foreach (Player t in Main.player)
        {
            if (t == null || !t.active) continue;
            var c = t.GetModPlayer<ChainedPlayer>();
            if (c.GrabberIndex == player.whoAmI && c.ActiveLeashItemType == Type)
                return true;
        }
        return false;
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

        if (IsLeashing(player))
        {
            // heavy when walking puppy :3
            Item.useTime = (int)(Item.useTime * 1.25f);
            Item.useAnimation = (int)(Item.useAnimation * 1.25f);
            Item.damage = (int)(BaseDamage * 0.65f);
            Item.knockBack = BaseKnockback * 0.7f;
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
            // right click finds a collared puppy under cursor
            float rangePx = LeashRangeTiles * 16f;
            foreach (Player target in Main.player)
            {
                if (target == null || !target.active || target.dead) continue;
                if (target.whoAmI == player.whoAmI) continue;
                if (!target.GetModPlayer<PuppyPlayer>().IsPuppy) continue;
                if (!target.GetModPlayer<ChainedPlayer>().hasCollar) continue;
                if (!target.Hitbox.Contains(Main.MouseWorld.ToPoint())) continue;
                if (Vector2.Distance(player.Center, target.Center) > rangePx) continue;

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
                        continue;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        ModContent.GetInstance<PuppyMod>().RequestLeashAttach(target.whoAmI, Type);
                    else
                        chain.SetGrabberAuthority(player.whoAmI, Type);
                }
                return true;
            }
            return false;
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
