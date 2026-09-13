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

namespace PuppyMod.Content.Items.Leash;

public class RopeLeashItem : SummonLeashItem
{
    public override string Texture => AssetUtils.GetWeaponTexturePathWithFallback(nameof(RopeLeashItem));

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
        Item.DefaultToWhip(ModContent.ProjectileType<RopeLeashProjectile>(), 8, 1f, 5);
        Item.useTime = 32;
        Item.useAnimation = 32;
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
        // H3: Returns only Attached+Puppy without re-adding range to avoid duplicate range insertion.
        // Range is handled exactly once in ModifyTooltips via Damage anchor; dedup via lineName check.
        yield return new TooltipLine(mod, "AttachedLabel", LabelColor(Language.GetTextValue("Mods.PuppyMod.Tooltips.Attached"), ColorAttachedLabel));
        yield return new TooltipLine(mod, "LeashPuppy", $"{LabelColor(Language.GetTextValue("Mods.PuppyMod.Tooltips.Puppy"), ColorPuppyLabel)} +15% movement speed");
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // H3: Unified single tooltip path – do not call base.ModifyTooltips to prevent order-sensitive duplicate range/Attached.
        // GetTooltipLines returns only Attached+Puppy (no range); range inserted once at Damage anchor or Price fallback with dedup.
        tooltips.StripVanity();
        int dmgIdx = tooltips.FindIndex(l => l.Name == "Damage" && l.Mod == "Terraria");
        if (dmgIdx >= 0 && !tooltips.Any(l => l.Mod == Mod.Name && l.Name == "LeashRange"))
            tooltips.Insert(dmgIdx + 1, new TooltipLine(Mod, "LeashRange", LabelColor($"{RangeTiles} leash range", ColorLeashRange)));
        // Apply Attached/Puppy lines exactly once; setBonus vs tooltip dedup already via lineName check in ApplyPuppyEquipmentTooltip – hjson lines intentional.
        bool hasAttached = tooltips.Any(l => l.Mod == Mod.Name && l.Name == "AttachedLabel");
        if (!hasAttached)
            tooltips.ApplyTooltips(Mod, this);
        // Fallback range if no Damage line existed – ensure range appears above Price.
        if (!tooltips.Any(l => l.Mod == Mod.Name && l.Name == "LeashRange"))
        {
            int priceIdx = tooltips.FindIndex(l => l.Name == "Price" && l.Mod == "Terraria");
            var rangeLine = new TooltipLine(Mod, "LeashRange", LabelColor($"{RangeTiles} leash range", ColorLeashRange));
            if (priceIdx >= 0) tooltips.Insert(priceIdx, rangeLine);
            else tooltips.Add(rangeLine);
        }
        tooltips.MovePriceToBottom();
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
