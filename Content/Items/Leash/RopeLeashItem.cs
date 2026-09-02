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

public class RopeLeashItem : BaseLeashItem
{
    public override int RangeTiles => 12;
    protected override DamageClass LeashDamageClass => DamageClass.SummonMeleeSpeed;
    protected override int BaseDamage => 8;
    protected override float BaseKnockback => 0.75f;
    protected override bool AppliesPenalty => false;

    public override string LeashTexturePath => "Terraria/Images/Chain34";
    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.78f,
        Stiffness: 0.14f,
        Damping: 0.72f,
        MaxStretchRatio: 1.42f,
        Curve: LeashElasticityCurve.Elastic,
        PuppyInertia: 1.15f,
        OwnerInertia: 0.22f
    );

    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<RopeLeashProjectile>(), 8, 0.75f, 9);
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(silver: 22);
    }

    public override bool MeleePrefix() => true;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float dir = 0.8f + 0.2f * Main.rand.NextFloat();
        if (Main.rand.NextBool(3)) dir *= -1.5f;
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, dir);
        return false;
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        foreach (var line in base.GetTooltipLines(mod))
            yield return line;
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Rope, 50)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void AffectPuppy(Player player)
    {
        player.moveSpeed += 0.15f;
    }
}
