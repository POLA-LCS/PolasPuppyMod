using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Data;
using PuppyMod.Common.Enums;
using PuppyMod.Content.Projectiles;

namespace PuppyMod.Content.Items.Leash;

public class ChainLeashItem : BaseLeashItem
{
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

    private static float PoisonChance => 0.20f;
    private static int PoisonDuration => 300;
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
        foreach (var line in base.GetTooltipLines(mod))
            yield return line;
        yield return new TooltipLine(mod, "ChainPoison", "Strike enemies to poison them");
        yield return new TooltipLine(mod, "LeashPenalty", "Weaker while puppy leashed") { OverrideColor = Color.LightGray };
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Chain, 20)
            .AddIngredient(ItemID.IronBar, 3)
            .AddIngredient(ItemID.Wood, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override void AffectPuppy(Player player)
    {
        player.statDefense += 5;
    }
}
