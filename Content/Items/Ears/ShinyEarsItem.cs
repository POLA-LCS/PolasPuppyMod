using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Tooltip;
using PuppyMod.Players;

namespace PuppyMod.Content.Items.Ears;

[AutoloadEquip(EquipType.Head)]
public class ShinyEarsItem : ModItem, IPuppyEars
{
    private const float BasePickSpeed = 0.12f;
    public PuppyEquipmentStats Stats => new(Defense: 0f, PickSpeed: BasePickSpeed);

    // Treasure shine / ore detection: functional 12 tiles half-box, vanity 6 tiles (halved). Light intensity also halved in vanity.
    // Vanity halving is handled locally via isVanity; central PuppyEquipment stats only halves PickSpeed – no duplicate range handling.
    // H4: Centralized via PuppyPlayer like ShinyTail – functional present skips vanity to avoid double light/scan; intentional stacking dedup.
    private const int ShineBoxHalf = 12;

    private const float BaseLightIntensity = 0.25f;
    private const float BaseLightIntensityVanity = BaseLightIntensity * 0.5f;

    /// <summary>Tile size in pixels (Terraria world unit: 16px per tile); 8f is half-tile centering offset.</summary>
    private const float TileSizePixels = 16f;
    private const float HalfTilePixels = 8f;

    public override string Texture => "PuppyMod/Assets/Armor/ShinyDogEarsArmor";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogEars);
        // CloneDefaults overwrites the autoloaded head slot with vanilla DogEars (242), which would render vanilla ears; restore the mod equip slot.
        Item.headSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 1, silver: 50);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        // H4: Centralize via PuppyPlayer like ShinyTail – flag functional, emission deduped in PuppyPlayer.PostUpdate.
        player.GetModPlayer<PuppyPlayer>().EnableShinyEars(isVanity: false);
    }

    public override void UpdateVanity(Player player)
    {
        // H4: If functional present skip vanity light – PuppyPlayer dedup ensures vanity only when no functional.
        player.GetModPlayer<PuppyPlayer>().EnableShinyEars(isVanity: true);
    }

    public override void UpdateEquip(Player player)
    {
        // H4: Head slot functional – same central path.
        player.GetModPlayer<PuppyPlayer>().EnableShinyEars(isVanity: false);
    }

    public void OnBark(Player player)
    {
        if (Main.dedServ) return;
        SpawnStarBurst(player);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.DogEars, 1)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.Daybloom, 5)
            .AddIngredient(ItemID.GoldOre, 20)
            .AddTile(TileID.WorkBenches)
            .Register();

        // Alternative platinum variant – same cost, avoids forcing Gold world.
        CreateRecipe()
            .AddIngredient(ItemID.DogEars, 1)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.Daybloom, 5)
            .AddIngredient(ItemID.PlatinumOre, 20)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        => tooltips.ApplyPuppyEquipmentTooltip(Mod, Item);

    private static void ApplyEffects(Player player, bool isVanity)
    {
        // Kept for backwards compat but not used – central path via PuppyPlayer dedups functional+vanity to single scan.
        EmitLight(player, isVanity);
        ShineTreasure(player, isVanity);
    }

    // H4: Exposed for central PuppyPlayer aggregation – functional takes precedence, vanity only if no functional. Single scan per tick avoids ~625*2.
    internal static void EmitLightForPlayer(Player player, bool isVanity) => EmitLight(player, isVanity);
    internal static void ShineTreasureForPlayer(Player player, bool isVanity) => ShineTreasure(player, isVanity);

    private static void EmitLight(Player player, bool isVanity)
    {
        if (Main.dedServ) return;
        // Halved light intensity in vanity (0.125 vs 0.25) – matches treasure shine halving; no central double-apply.
        // H4 dedup: functional+vanity both equipped does light once via PuppyPlayer (functional precedence), not twice.
        float i = isVanity ? BaseLightIntensityVanity : BaseLightIntensity;
        Lighting.AddLight(player.Center, new Vector3(i, i * 0.85f, i * 0.35f));
    }

    private static void ShineTreasure(Player player, bool isVanity)
    {
        if (Main.dedServ) return;
        float i = isVanity ? BaseLightIntensityVanity : BaseLightIntensity;
        int range = isVanity ? ShineBoxHalf / 2 : ShineBoxHalf;
        Vector3 color = new(i, i * 0.85f, i * 0.35f);
        Point center = player.Center.ToTileCoordinates();
        for (int x = center.X - range; x <= center.X + range; x++)
        for (int y = center.Y - range; y <= center.Y + range; y++)
        {
            if (!WorldGen.InWorld(x, y)) continue;
            Tile tile = Main.tile[x, y];
            if (!tile.HasTile || tile.IsActuated) continue;
            if (Main.tileSpelunker[tile.TileType])
                Lighting.AddLight(new Vector2(x * TileSizePixels + HalfTilePixels, y * TileSizePixels + HalfTilePixels), color);
        }
    }

    private static void SpawnStarBurst(Player player)
    {
        Vector2 mouth = player.Center + new Vector2(player.direction * 6f, -14f);
        for (int i = 0; i < 20; i++)
        {
            float speed = Main.rand.NextFloat(1f, 6f);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 vel = new Vector2(player.direction * speed, 0).RotatedBy(angle) + new Vector2(0, Main.rand.NextFloat(-2f, 1f));
            Dust d = Dust.NewDustPerfect(mouth, DustID.YellowStarDust, vel, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
            d.noGravity = true;
            d.fadeIn = 0.2f;
            Lighting.AddLight(d.position, new Vector3(0.15f, 0.12f, 0.05f));
        }
    }
}
