using PetAnyone;
using Terraria.ModLoader;

namespace PuppyMod.Services.Petting;

/// <summary>
/// Clears the utility's registrations and per-target state when the mod unloads. Registrations
/// owned by other mods are pruned lazily through <see cref="PetRegistry.IsOwnerLoaded"/>.
/// </summary>
internal sealed class PettingSystem : ModSystem
{
    public override void Unload()
    {
        PetEvents.Clear();
        PetRegistry.Clear();
        PetService.Clear();
    }
}
