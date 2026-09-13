using Terraria;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;

namespace PuppyMod.Services.Leash;

public static class PuppyLeashBonusService
{
    // Defense aggregation happens in PostUpdateEquips (equipment stats + pair bonus) to avoid late-sweep flicker.
    // Individual stats stack additively; the attached pair bonus uses the strongest applicable effect per player.
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
}
