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

public class RubberLeashItem : BaseLeashItem
{
    public override int RangeTiles => 10;
    protected override DamageClass LeashDamageClass => DamageClass.SummonMeleeSpeed;
    protected override int BaseDamage => 14;
    protected override float BaseKnockback => 2.5f;

    public const string RubberTexture = "Terraria/Images/Chain";
    public override string LeashTexturePath => RubberTexture;
    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.42f,
        Stiffness: 0.035f,
        Damping: 0.18f,
        MaxStretchRatio: 1.70f,
        Curve: LeashElasticityCurve.ElasticBounce,
        PuppyInertia: 0.60f,
        OwnerInertia: 0.26f
    );

    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<ChainLeashProjectile>(), 14, 2.5f, 4);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 60);
    }

    public override bool MeleePrefix() => true;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float dir = 0.8f + 0.4f * Main.rand.NextFloat();
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, dir);
        return false;
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        foreach (var line in base.GetTooltipLines(mod))
            yield return line;
        yield return new TooltipLine(mod, "RubberBounce", "Bouncy stretch");
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Gel, 50)
            .AddIngredient(ItemID.Vine, 2)
            .AddIngredient(ItemID.Wood, 10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void AffectPuppy(Player player)
    {
        // No extra effect; elastic leash lets puppy move freely.
    }
}
