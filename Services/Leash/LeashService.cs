using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Utils;
using PuppyMod.Players;

namespace PuppyMod.Services.Leash;

public static class LeashService
{
    public static bool IsLeashing(Player owner, int leashType)
    {
        foreach (Player t in Main.player)
        {
            if (t == null || !t.active) continue;
            var c = t.GetModPlayer<ChainedPlayer>();
            if (c.GrabberIndex == owner.whoAmI && c.ActiveLeashItemType == leashType)
                return true;
        }
        return false;
    }

    public static bool IsLeashingAny(Player owner)
    {
        foreach (Player t in Main.player)
        {
            if (t == null || !t.active) continue;
            var c = t.GetModPlayer<ChainedPlayer>();
            if (c.GrabberIndex == owner.whoAmI)
                return true;
        }
        return false;
    }

    public static Player FindPuppyUnderCursor(Player owner, int rangeTiles)
    {
        float rangePx = rangeTiles * 16f;
        foreach (Player target in Main.player)
        {
            if (target == null || !target.active || target.dead) continue;
            if (target.whoAmI == owner.whoAmI) continue;
            if (!target.GetModPlayer<PuppyPlayer>().IsPuppy) continue;
            if (!target.GetModPlayer<ChainedPlayer>().hasCollar) continue;
            if (!target.Hitbox.Contains(Main.MouseWorld.ToPoint())) continue;
            if (!DistanceUtils.WithinPixels(owner.Center, target.Center, rangePx)) continue;
            return target;
        }
        return null;
    }

    public static bool CanAttach(Player owner, Player target, int leashType)
    {
        if (owner.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
        if (!target.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
        if (!target.GetModPlayer<ChainedPlayer>().hasCollar) return false;
        if (ModContent.GetModItem(leashType) is not ILeashItem leash) return false;
        if (owner.HeldItem.type != leashType) return false;
        if (!DistanceUtils.WithinTiles(owner, target, leash.RangeTiles)) return false;
        var chain = target.GetModPlayer<ChainedPlayer>();
        if (chain.GrabberIndex.HasValue && chain.GrabberIndex != owner.whoAmI) return false;
        return true;
    }
}
