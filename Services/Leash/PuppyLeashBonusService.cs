using System.Collections.Generic;
using Terraria;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;

namespace PuppyMod.Services.Leash;

public static class PuppyLeashBonusService
{
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

    public static bool TryGetKnockbackMultiplier(Player player, out float multiplier)
    {
        multiplier = 1f;
        if (player == null || !player.active || player.dead)
            return false;

        bool found = false;
        PuppyLeashBonusEffect strongest = default;
        foreach (Player puppy in Main.player)
        {
            if (!TryGetAttachedPairEffect(puppy, out Player owner, out PuppyLeashBonusEffect effect))
                continue;
            if (puppy.whoAmI != player.whoAmI && owner.whoAmI != player.whoAmI)
                continue;

            if (!found || effect.IsStrongerThan(strongest))
            {
                strongest = effect;
                found = true;
            }
        }

        if (!found)
            return false;

        multiplier = strongest.KnockbackMultiplier;
        return true;
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
