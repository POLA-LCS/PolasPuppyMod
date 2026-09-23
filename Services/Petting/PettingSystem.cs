using PetAnyone;
using Terraria.ModLoader;

namespace PuppyMod.Services.Petting;

/// <summary>
/// Owner scoped cleanup of this mod's registrations and subscriptions on unload. The API mod
/// clears its own global state on unload, so only this mod's entries are removed here.
/// </summary>
internal sealed class PettingSystem : ModSystem
{
    public override void Unload()
    {
        PetEvents.ClearOwner(Mod);
        PetRegistry.ClearOwner(Mod);
    }
}
