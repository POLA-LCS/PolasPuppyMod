using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Buffs;
using PuppyMod.Content.Items.Clickers;
using PuppyMod.Players;

namespace PuppyMod.Services.Pat;

public static class PatService
{
    public const int PatRangeTiles = 3;
    public const int PatBuffTicks = 180;
    public const int PatCooldownTicks = 20;
    public const int PatWagDurationTicks = 90;
    public const int PatReachDurationTicks = 30;

    private const string PatHandTexturePath = "PuppyMod/Assets/Pat/PatHand";

    public static readonly SoundPad Pats = SoundPad.LoadCategory("PuppySounds/pat", volume: 0.9f);

    /// <summary>Whether this player's right-click is free for patting (empty hand, or an item with no right-click usage).</summary>
    public static bool IsPatHand(Player patter)
    {
        Item held = patter.HeldItem;
        return held.IsAir || held.ModItem is BaseClickerItem;
    }

    /// <summary>Attempts a pat from the local player towards the puppy under the cursor.</summary>
    public static void TryPat(Player patter)
    {
        Player target = FindPuppyUnderCursor(patter);
        if (target == null)
            return;

        var puppy = patter.GetModPlayer<PuppyPlayer>();
        if (Main.GameUpdateCount - puppy.PatterPatTick < PatCooldownTicks)
            return;
        puppy.PatterPatTick = (int)Main.GameUpdateCount;

        // Reach the hand toward the puppy while patting.
        puppy.PatReachTicks = PatReachDurationTicks;
        puppy.PatReachTarget = target.whoAmI;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModContent.GetInstance<PuppyMod>().RequestPat(target.whoAmI);
        }
        else if (Main.netMode == NetmodeID.Server)
        {
            if (CanPat(patter, target) && ApplyPat(target))
                ModContent.GetInstance<PuppyMod>().BroadcastPat(target.whoAmI);
        }
        else if (ApplyPat(target))
        {
            PlayPatEffects(target);
            target.GetModPlayer<PuppyPlayer>().PatWagTicks = PatWagDurationTicks;
        }
    }

    public static Player FindPuppyUnderCursor(Player patter)
    {
        float rangePx = PatRangeTiles * DistanceUtils.TilePixels;
        foreach (Player target in Main.player)
        {
            if (target == null || !target.active || target.dead)
                continue;
            if (target.whoAmI == patter.whoAmI)
                continue;
            if (!target.GetModPlayer<PuppyPlayer>().IsPuppy)
                continue;
            if (!target.Hitbox.Contains(Main.MouseWorld.ToPoint()))
                continue;
            if (!DistanceUtils.WithinPixels(patter.Center, target.Center, rangePx))
                continue;
            return target;
        }

        return null;
    }

    /// <summary>Server-side validation for a pat request.</summary>
    public static bool CanPat(Player patter, Player target)
    {
        if (target == null || !target.active || target.dead)
            return false;
        if (target.whoAmI == patter.whoAmI)
            return false;
        if (!target.GetModPlayer<PuppyPlayer>().IsPuppy)
            return false;
        if (!DistanceUtils.WithinTiles(patter, target, PatRangeTiles))
            return false;

        return true;
    }

    /// <summary>Applies the Good Puppy buff and the cooldown to the target. Returns whether the pat was applied.</summary>
    public static bool ApplyPat(Player target)
    {
        var puppy = target.GetModPlayer<PuppyPlayer>();
        if (Main.GameUpdateCount - puppy.LastPatTick < PatCooldownTicks)
            return false;

        puppy.LastPatTick = (int)Main.GameUpdateCount;
        puppy.PatWagTicks = PatWagDurationTicks;
        target.AddBuff(ModContent.BuffType<GoodPuppyBuff>(), PatBuffTicks);
        return true;
    }

    /// <summary>Whether a patting hand texture is available (drop one at Assets/Pat/PatHand.png).</summary>
    public static bool TryGetPatHandTexture(out Texture2D texture)
    {
        texture = null;
        if (Main.dedServ || !ModContent.HasAsset(PatHandTexturePath))
            return false;

        texture = ModContent.Request<Texture2D>(PatHandTexturePath, AssetRequestMode.ImmediateLoad).Value;
        return texture != null;
    }

    /// <summary>Hearts and pat sounds around the puppy. Runs on whichever client needs to show it.</summary>
    public static void PlayPatEffects(Player target)
    {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 7; i++)
        {
            Vector2 position = target.Center
                + new Vector2(0f, -target.height * 0.6f)
                + Main.rand.NextVector2Circular(12f, 10f);

            Dust heart = Dust.NewDustPerfect(
                position,
                DustID.HeartCrystal,
                new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -1.2f),
                0,
                default,
                Main.rand.NextFloat(0.9f, 1.3f));

            heart.noGravity = true;
        }

        if (Pats.Count != 0)
            SoundEngine.PlaySound(Pats.GetRandom(), target.Center);
    }
}
