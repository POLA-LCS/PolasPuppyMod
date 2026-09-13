using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using PuppyMod.Common.Physics;
using PuppyMod.Common.Tooltip;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Projectiles;
using static PuppyMod.Common.Tooltip.TooltipExtensions;

namespace PuppyMod.Content.Items.Leashes;

public class ChainLeashItem : SummonLeashItem
{
    public override string Texture => AssetUtils.GetWeaponTexturePathWithFallback(nameof(ChainLeashItem));

    public override int RangeTiles => 10;
    protected override DamageClass LeashDamageClass => DamageClass.SummonMeleeSpeed;
    protected override int BaseDamage => 17;
    protected override float BaseKnockback => 2f;

    public override string LeashTexturePath => "Terraria/Images/Chain22";
    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.92f,
        Stiffness: 0.34f,
        Damping: 0.85f,
        MaxStretchRatio: 1.06f,
        Curve: LeashElasticityCurve.PasitoAPasito,
        PuppyInertia: 1.80f,
        OwnerInertia: 0.12f
    );

    private const float PoisonChance = 0.20f;
    private const int PoisonDuration = 300;
    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<ChainLeashProjectile>(), 17, 5f, 4);
        Item.useTime = 45;
        Item.useAnimation = 45;
        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(silver: 47);
    }

    public override bool MeleePrefix() => true;

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.rand.NextFloat() < PoisonChance)
            target.AddBuff(BuffID.Poisoned, PoisonDuration);
    }

    public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
    {
        if (Main.rand.NextFloat() < PoisonChance)
            target.AddBuff(BuffID.Poisoned, PoisonDuration);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float dir = 0.6f + 0.4f * Main.rand.NextFloat();
        if (Main.rand.NextBool(3)) dir *= -2.5f;
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, dir);
        return false;
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        // H3: Returns only effect+Attached+Puppy without re-adding range to avoid duplicate range insertion.
        yield return new TooltipLine(mod, "ChainPoison", "May poison foes");
        yield return new TooltipLine(mod, "LeashPenalty", "Weaker while leashing");
        yield return new TooltipLine(mod, "AttachedLabel", ColorizeLabel(Language.GetTextValue("Mods.PuppyMod.Tooltips.Attached"), ColorAttachedLabel));
        yield return new TooltipLine(mod, "LeashPuppyDefense", $"{ColorizeLabel(Language.GetTextValue("Mods.PuppyMod.Tooltips.Puppy"), ColorPuppyLabel)} +5 defense");
        yield return new TooltipLine(mod, "LeashPuppySlow", $"{ColorizeLabel(Language.GetTextValue("Mods.PuppyMod.Tooltips.Puppy"), ColorPuppyLabel)} -5% movement speed");
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // H3: Unified single tooltip path – do not call base.ModifyTooltips to prevent order-sensitive duplicate range/Attached.
        // GetTooltipLines returns only effect+Attached+Puppy (no range); range inserted once at Damage anchor or Price fallback with dedup.
        tooltips.StripVanity();
        int dmgIdx = tooltips.FindIndex(l => l.Name == "Damage" && l.Mod == "Terraria");
        if (dmgIdx >= 0 && !tooltips.Any(l => l.Mod == Mod.Name && l.Name == "LeashRange"))
            tooltips.Insert(dmgIdx + 1, new TooltipLine(Mod, "LeashRange", ColorizeLabel($"{RangeTiles} leash range", ColorLeashRange)));
        // Apply effect+Attached+Puppy lines exactly once; setBonus vs tooltip dedup already via lineName check – hjson lines intentional.
        bool hasAttached = tooltips.Any(l => l.Mod == Mod.Name && l.Name == "AttachedLabel");
        if (!hasAttached)
            tooltips.ApplyTooltips(Mod, this);
        // Fallback range if no Damage line existed – ensure range appears above Price.
        if (!tooltips.Any(l => l.Mod == Mod.Name && l.Name == "LeashRange"))
        {
            int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
            var rangeLine = new TooltipLine(Mod, "LeashRange", ColorizeLabel($"{RangeTiles} leash range", ColorLeashRange));
            if (priceIdx >= 0) tooltips.Insert(priceIdx, rangeLine);
            else tooltips.Add(rangeLine);
        }
        tooltips.MovePriceToBottom();
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Chain, 20)
            .AddRecipeGroup(RecipeGroupID.IronBar, 3)
            .AddRecipeGroup(RecipeGroupID.Wood, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override void AffectPuppy(Player player)
    {
        player.statDefense += 5;
        player.moveSpeed -= 0.05f;
    }
}
