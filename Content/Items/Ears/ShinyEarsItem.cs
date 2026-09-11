using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Tooltip;
using PuppyMod.Players;

namespace PuppyMod.Content.Items.Ears;

public class ShinyEarsItem : ModItem, IPuppyEars
{
    private const float BasePickSpeed = 0.12f;
    public float PickSpeedAccessory => BasePickSpeed;
    public float PickSpeedVanity => BasePickSpeed * 0.5f;

    private const int ShineBoxHalf = 12;

    private const float BaseLightIntensity = 0.25f;
    private const float BaseLightIntensityVanity = BaseLightIntensity * 0.5f;

    public override string Texture => "Terraria/Images/Item_" + ItemID.DogEars;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogEars);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 1, silver: 50);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<PuppyPlayer>().DogEarsAccessoryType = Type;
        ApplyEffects(player, isVanity: false);
    }

    public override void UpdateVanity(Player player)
    {
        player.GetModPlayer<PuppyPlayer>().DogEarsVanityType = Type;
        ApplyEffects(player, isVanity: true);
    }

    public override void UpdateEquip(Player player)
    {
        player.GetModPlayer<PuppyPlayer>().DogEarsAccessoryType = Type;
        ApplyEffects(player, isVanity: false);
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
    }

    private const string DigSpeedText = "Increase digging speed";
    private const string ShinyDefault = "Now you are shiny";
    private const string ShinyQuote = "'I sniff treasures everywhere! ^OwO^'";
    private const string HalvedPrefix = "halved: ";

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.StripVanity();
        tooltips.ApplyPuppyFlavor(Mod);

        int index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Tooltip0");
        if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name.StartsWith("Tooltip"));
        if (index == -1) index = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Defense");
        if (index == -1) index = tooltips.Count - 1;

        bool equipped = false;
        bool inVanity = false;
        if (Main.LocalPlayer != null && Main.LocalPlayer.active)
        {
            var polas = Main.LocalPlayer.GetModPlayer<PolasBasePlayer>();
            equipped = polas.HasInAccessory(Type) || polas.HasInVanity(Type);
            inVanity = polas.HasInVanity(Type);
        }

        string prefix = inVanity ? HalvedPrefix : string.Empty;

        tooltips.Insert(index + 1, new TooltipLine(Mod, "ShinyEarsStat", prefix + DigSpeedText));
        tooltips.Insert(index + 2, new TooltipLine(Mod, "ShinyEarsDefault", prefix + ShinyDefault));
        tooltips.Insert(index + 3, new TooltipLine(Mod, "ShinyEarsQuote", ShinyQuote));

        tooltips.InsertPuppyBonus(Mod, equipped, index + 4);
        tooltips.MovePriceToBottom();
    }

    private void ApplyEffects(Player player, bool isVanity)
    {
        EmitLight(player, isVanity);
        ShineTreasure(player, isVanity);
    }

    private void EmitLight(Player player, bool isVanity)
    {
        if (Main.dedServ) return;
        float i = isVanity ? BaseLightIntensityVanity : BaseLightIntensity;
        Lighting.AddLight(player.Center, new Vector3(i, i * 0.85f, i * 0.35f));
    }

    private void ShineTreasure(Player player, bool isVanity)
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
                Lighting.AddLight(new Vector2(x * 16f + 8f, y * 16f + 8f), color);
        }
    }

    private void SpawnStarBurst(Player player)
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
