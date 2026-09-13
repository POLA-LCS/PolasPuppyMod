using Terraria.ModLoader;

namespace PuppyMod.Services.Leash;

public sealed class PuppyLeashBonusSystem : ModSystem
{
    public override void PostUpdatePlayers()
    {
        PuppyLeashBonusService.ApplyAttachedEffects();
    }
}
