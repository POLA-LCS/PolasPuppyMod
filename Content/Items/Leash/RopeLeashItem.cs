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
    public override int RangeTiles => 14;
    protected override DamageClass LeashDamageClass => DamageClass.Melee;
    protected override int BaseDamage => 18;
    protected override float BaseKnockback => 3f;

    public const string RopeTexture = "Terraria/Images/Chain";
    public override string LeashTexturePath => RopeTexture;
    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.78f,
        Stiffness: 0.12f,
        Damping: 0.55f,
        MaxStretchRatio: 1.18f,
        Curve: LeashElasticityCurve.SmoothRamp,
        PuppyInertia: 1.00f,
        OwnerInertia: 0.18f
    );

    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<ChainLeashProjectile>(), 18, 3f, 4);
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(silver: 80);
    }

    public override bool MeleePrefix() => true;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false;
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        foreach (var line in base.GetTooltipLines(mod))
            yield return line;
        yield return new TooltipLine(mod, "RopeSteady", "Steady pull");
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.Rope, 30)
            .AddIngredient(ItemID.Wood, 10)
            .AddIngredient(ItemID.Silk, 2)
            .AddTile(TileID.Loom)
            .Register();
    }

    public override void AffectPuppy(Player player)
    {
        // No extra effect; steady handling keeps puppy stable.
    }
}
