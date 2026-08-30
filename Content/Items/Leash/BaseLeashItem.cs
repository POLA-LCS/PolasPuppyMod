using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Leash;

public abstract class BaseLeashItem : ModItem
{
    public abstract int LeashRangeTiles { get; }
    public virtual int LeashDefenseBonus => 0;
    public virtual float PoisonChance => 0f;
    public virtual int PoisonDuration => 300;
    protected abstract DamageClass LeashDamageClass { get; }
    protected virtual int BaseDamage => 18;
    protected virtual float BaseKnockback => 3f;

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
        if (player.GetModPlayer<PuppyPlayer>().IsPuppy)
            return false;

        if (player.altFunctionUse == 2)
        {
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.useTime = 12;
            Item.useAnimation = 12;
        }
        else
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 22;
            Item.useAnimation = 22;
        }
        return base.CanUseItem(player);
    }

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
                if (!target.GetModPlayer<ChainedPlayer>().hasChainLeash) continue;
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

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (PoisonChance > 0f && Main.rand.NextFloat() < PoisonChance)
            target.AddBuff(BuffID.Poisoned, PoisonDuration);
    }

    public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (PoisonChance > 0f && Main.rand.NextFloat() < PoisonChance)
            target.AddBuff(BuffID.Poisoned, PoisonDuration);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        int idx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
        var rangeLine = new TooltipLine(Mod, "LeashRange", $"{LeashRangeTiles} leash range");
        rangeLine.OverrideColor = new Color(193, 154, 107);
        if (idx >= 0) tooltips.Insert(idx, rangeLine);
        else tooltips.Add(rangeLine);

        if (LeashDefenseBonus > 0)
        {
            var defLine = new TooltipLine(Mod, "LeashDefense", $"+{LeashDefenseBonus} defense to leashed puppy");
            defLine.OverrideColor = Color.LightGreen;
            tooltips.Add(defLine);
        }

        if (PoisonChance > 0f)
        {
            var poiLine = new TooltipLine(Mod, "LeashPoison", $"has chance to inflict Poisoned");
            poiLine.OverrideColor = Color.LightGreen;
            tooltips.Add(poiLine);
        }

        if (LeashExtraDescription != null)
        {
            var extra = new TooltipLine(Mod, "LeashExtra", LeashExtraDescription);
            extra.OverrideColor = Color.LightGreen;
            tooltips.Add(extra);
        }
    }

    protected virtual string LeashExtraDescription => null;
}
