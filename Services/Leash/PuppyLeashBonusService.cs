using System.Collections.Generic;
using Terraria;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;

namespace PuppyMod.Services.Leash;

public static class PuppyLeashBonusService
{
    // H2 fix: Defense timing split – all defense aggregated in PostUpdateEquips (PuppyPlayer/ChainedPlayer), not ModSystem.PostUpdatePlayers late.
    // Order: ResetEffects → PostUpdateEquips aggregation (equipment stats + leash AffectPuppy + pair bonus) → no late flicker.
    // Stacking: individual PuppyEquipmentStats stack additively; attached pair bonus uses strongest per player (individual stack, pair strongest).
    // Keep ApplyAttachedEffects for backwards compat but deprecated – now per-player via ApplyDefenseForPlayer called in PostUpdateEquips.
    [System.Obsolete("H2: Use ApplyDefenseForPlayer in PostUpdateEquips to aggregate with equipment stats; global late sweep causes flicker.")]
    public static void ApplyAttachedEffects()
    {
        Dictionary<int, PuppyLeashBonusEffect> strongestEffects = CollectStrongestEffects();
        foreach (KeyValuePair<int, PuppyLeashBonusEffect> entry in strongestEffects)
        {
            Player player = GetPlayer(entry.Key);
            if (player == null || !player.active || player.dead)
                continue;

            player.statDefense += entry.Value.DefenseBonus;
        }
    }

    /// <summary>H2: Per-player pair bonus aggregated in PostUpdateEquips with equipment stats – no late flicker, strongest wins.</summary>
    public static void ApplyDefenseForPlayer(Player player)
    {
        if (player == null || !player.active || player.dead) return;
        if (TryGetStrongestEffectForPlayer(player, out PuppyLeashBonusEffect effect) && effect.HasEffect)
            player.statDefense += effect.DefenseBonus;
    }

    public static bool TryGetStrongestEffectForPlayer(Player player, out PuppyLeashBonusEffect effect)
    {
        effect = default;
        if (player == null || !player.active || player.dead) return false;
        bool found = false;
        PuppyLeashBonusEffect strongest = default;
        foreach (Player puppy in Main.player)
        {
            if (!TryGetAttachedPairEffect(puppy, out Player owner, out PuppyLeashBonusEffect candidate))
                continue;
            if (puppy.whoAmI != player.whoAmI && owner.whoAmI != player.whoAmI)
                continue;
            if (!found || candidate.IsStrongerThan(strongest))
            {
                strongest = candidate;
                found = true;
            }
        }
        if (!found) return false;
        effect = strongest;
        return effect.HasEffect;
    }

    public static bool TryGetKnockbackMultiplier(Player player, out float multiplier)
    {
        multiplier = 1f;
        if (TryGetStrongestEffectForPlayer(player, out PuppyLeashBonusEffect strongest))
        {
            multiplier = strongest.KnockbackMultiplier;
            return true;
        }
        return false;
    }

    private static Dictionary<int, PuppyLeashBonusEffect> CollectStrongestEffects()
    {
        var strongestEffects = new Dictionary<int, PuppyLeashBonusEffect>();
        foreach (Player puppy in Main.player)
        {
            if (!TryGetAttachedPairEffect(puppy, out Player owner, out PuppyLeashBonusEffect effect))
                continue;

            AddStrongest(strongestEffects, puppy, effect);
            AddStrongest(strongestEffects, owner, effect);
        }

        return strongestEffects;
    }

    private static void AddStrongest(
        Dictionary<int, PuppyLeashBonusEffect> effects,
        Player player,
        PuppyLeashBonusEffect candidate)
    {
        if (player == null || !player.active || player.dead || !candidate.HasEffect)
            return;

        if (!effects.TryGetValue(player.whoAmI, out PuppyLeashBonusEffect current)
            || candidate.IsStrongerThan(current))
        {
            effects[player.whoAmI] = candidate;
        }
    }

    private static bool TryGetAttachedPairEffect(
        Player puppy,
        out Player owner,
        out PuppyLeashBonusEffect effect)
    {
        owner = null;
        effect = default;
        if (puppy == null || !puppy.active || puppy.dead)
            return false;

        if (!LeashService.TryGetAttachedOwner(puppy, out owner))
            return false;

        PuppyEquipmentResolution resolution = puppy.GetModPlayer<PuppyPlayer>().EquipmentResolution;
        if (!resolution.TryGetPairBonus(out PuppyPairBonusDefinition pairBonus))
            return false;

        effect = pairBonus.GetEffect(resolution.SelectedEars, resolution.SelectedTail);
        return effect.HasEffect;
    }

    private static Player GetPlayer(int whoAmI)
    {
        if (whoAmI < 0 || whoAmI >= Main.player.Length)
            return null;
        return Main.player[whoAmI];
    }
}
