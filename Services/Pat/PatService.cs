using Microsoft.Xna.Framework;
using System.Collections.Generic;
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
    public const int PatHeartIntervalTicks = 30;
    public const int PatBuffRefreshTicks = 10;
    public const float PatAngleMorphed = 0.27f;
    public const float PatAngleVanilla = 0.37f;

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

        // Reach the arm toward the puppy while patting.
        puppy.PatReachTicks = PatReachDurationTicks;
        puppy.PatReachTarget = target.whoAmI;
        patter.direction = target.Center.X >= patter.Center.X ? 1 : -1;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModContent.GetInstance<PuppyMod>().RequestPat(target.whoAmI);
        }
        else if (Main.netMode == NetmodeID.Server)
        {
            if (CanPat(patter, target) && ApplyPat(target))
                ModContent.GetInstance<PuppyMod>().BroadcastPat(patter.whoAmI, target.whoAmI);
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
        if (patter == null || !patter.active || patter.dead)
            return false;
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
    public static bool ApplyPat(Player target, bool bypassCooldown = false)
    {
        var puppy = target.GetModPlayer<PuppyPlayer>();
        if (!bypassCooldown && Main.GameUpdateCount - puppy.LastPatTick < PatCooldownTicks)
            return false;

        puppy.LastPatTick = (int)Main.GameUpdateCount;
        puppy.PatWagTicks = PatWagDurationTicks;
        target.AddBuff(ModContent.BuffType<GoodPuppyBuff>(), PatBuffTicks);
        return true;
    }

    /// <summary>Applies the Good Puppy buff while holding, bypassing the tap cooldown.</summary>
    public static bool ApplyPatHold(Player target)
    {
        return ApplyPat(target, bypassCooldown: true);
    }

    /// <summary>Gore type 331 is the full bright heart used by the Love Potion.</summary>
    private const int LoveHeartGore = 331;

    /// <summary>Pat sound around the puppy. Hearts are spawned separately so their rate stays chill.</summary>
    public static void PlayPatEffects(Player target)
    {
        if (Main.dedServ)
            return;

        if (Pats.Count != 0)
            SoundEngine.PlaySound(Pats.GetRandom(), target.Center);
    }

    private static readonly Dictionary<int, int> LastHeartTick = new();

    /// <summary>
    /// Spawns at most one heart per target every PatHeartIntervalTicks, no matter how often
    /// the pat/hold refresh or the network broadcast asks for one.
    /// </summary>
    public static void PlayPatHeartSynced(Player target)
    {
        if (Main.dedServ)
            return;

        int now = (int)Main.GameUpdateCount;
        if (LastHeartTick.TryGetValue(target.whoAmI, out int last) && now - last < PatHeartIntervalTicks)
            return;

        LastHeartTick[target.whoAmI] = now;
        SpawnPatHeart(target);
    }

    /// <summary>Spawns one Love Potion style full heart that gently floats up over the puppy's head.</summary>
    private static void SpawnPatHeart(Player target)
    {
        Vector2 position = target.Center
            + new Vector2(0f, -target.height * 0.6f)
            + Main.rand.NextVector2Circular(6f, 4f);

        int index = Gore.NewGore(
            target.GetSource_FromThis(),
            position,
            Vector2.Zero,
            LoveHeartGore,
            Main.rand.NextFloat(0.55f, 0.85f));

        if (index < 0 || index >= Main.gore.Length)
            return;

        Gore heart = Main.gore[index];
        heart.sticky = false;
        heart.velocity = new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), Main.rand.NextFloat(-0.9f, -0.5f));
        heart.rotation = 0f;
        heart.timeLeft = 70;
    }
}
