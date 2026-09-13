using Terraria.ModLoader;

namespace PuppyMod.Services.Leash;

public sealed class PuppyLeashBonusSystem : ModSystem
{
    // H2 fix: Disabled late ModSystem.PostUpdatePlayers sweep – defense now aggregated in PuppyPlayer.PostUpdateEquips via PuppyLeashBonusService.ApplyDefenseForPlayer.
    // Keeping class as no-op to avoid breaking existing world saves referencing this system; do not re-enable late defense.
    public override void PostUpdatePlayers()
    {
        // Intentionally empty – see PuppyLeashBonusService.ApplyDefenseForPlayer for PostUpdateEquips aggregation.
    }
}
