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

public class ShinyEarsItem : ModItem, IPuppyEars
{
    private const float BasePickSpeed = 0.12f;
    public PuppyEquipmentStats Stats => new(Defense: 0f, PickSpeed: BasePickSpeed);

    // Individual light: full intensity functional, half in vanity. Ore sight belongs to the Shiny pair bonus now.
    private const float BaseLightIntensity = 0.25f;
    private const float BaseLightIntensityVanity = BaseLightIntensity * 0.5f;

    public override string Texture => "PuppyMod/Assets/Armor/ShinyDogEarsArmor";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogEars);
        // CloneDefaults resets headSlot to vanilla DogEars (242); restore the animated equip slot.
        Item.headSlot = EquipLoader.GetEquipSlot(Mod, PuppyEquipmentTextures.ShinyEarsHead, EquipType.Head);
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 1, silver: 50);
    }

    public override void SetStaticDefaults()
    {
        // Keep the player's hair visible under the ears.
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
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

    /// <summary>Central light emission – functional takes precedence over vanity via PuppyPlayer.</summary>
    internal static void EmitLightForPlayer(Player player, bool isVanity) => EmitLight(player, isVanity);

    private static void EmitLight(Player player, bool isVanity)
    {
        if (Main.dedServ) return;
        // Halved light intensity in vanity (0.125 vs 0.25) – matches treasure shine halving; no central double-apply.
        // H4 dedup: functional+vanity both equipped does light once via PuppyPlayer (functional precedence), not twice.
        float i = isVanity ? BaseLightIntensityVanity : BaseLightIntensity;
        Lighting.AddLight(player.Center, new Vector3(i, i * 0.85f, i * 0.35f));
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
