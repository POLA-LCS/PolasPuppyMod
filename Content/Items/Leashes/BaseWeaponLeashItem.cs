using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Physics;
using PuppyMod.Common.Tooltip;
using PuppyMod.Content.GlobalItems;
using static PuppyMod.Common.Tooltip.TooltipExtensions;
using PuppyMod.Services.Leash;

namespace PuppyMod.Content.Items.Leashes;

public abstract class BaseWeaponLeashItem : ModItem, ILeashItem, ITooltipProvider
{
    public const float PenaltyUseTimeMult = 1.25f;
    public const float PenaltyDamageMult = 0.65f;
    public const float PenaltyKnockMult = 0.7f;

    public abstract int RangeTiles { get; }
    public abstract string LeashTexturePath { get; }
    public virtual LeashPhysicsProfile Physics => new();
    protected abstract DamageClass LeashDamageClass { get; }
    protected virtual int BaseDamage => 18;
    protected virtual float BaseKnockback => 3f;
    protected virtual bool AppliesPenalty => true;

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

    private void EnsureOriginalCached(Item item)
    {
        var g = item.GetGlobalItem<WeaponLeashGlobalItem>();
        if (g.HasOriginal) return;
        g.OriginStyle = item.useStyle;
        g.OrigTime = item.useTime;
        g.OrigAnim = item.useAnimation;
        g.HasOriginal = true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.GetModPlayer<Players.PuppyPlayer>().IsPuppy)
            return false;

        // Per-item cache stored on the GlobalItem instance.
        var g = Item.GetGlobalItem<WeaponLeashGlobalItem>();
        EnsureOriginalCached(Item);

        if (player.altFunctionUse == 2)
        {
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.useTime = 12;
            Item.useAnimation = 12;
        }
        else if (g.HasOriginal)
        {
            Item.useStyle = g.OriginStyle;
            Item.useTime = g.OrigTime;
            Item.useAnimation = g.OrigAnim;
        }

        if (AppliesPenalty && LeashService.IsLeashing(player, Type))
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
            bool toggled = LeashAttachService.TryToggleLeash(player, Type, RangeTiles);
            return toggled;
        }
        return true;
    }

    public virtual IEnumerable<TooltipLine> GetTooltipLines(Mod mod) => Enumerable.Empty<TooltipLine>();

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        int dmgIdx = tooltips.FindIndex(l => l.Name == "Damage" && l.Mod == "Terraria");
        if (dmgIdx >= 0)
            tooltips.Insert(dmgIdx + 1, new TooltipLine(Mod, "LeashRange", ColorizeLabel($"{RangeTiles} leash range", ColorLeashRange)));
        else
            tooltips.ApplyTooltips(Mod, this);
    }

    public virtual void AffectPuppy(Player puppy) { }
}
