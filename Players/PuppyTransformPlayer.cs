#nullable enable
using PuppyMod.Content.Transformations;
using TransLib.Core;
using TransLib.Core.Transforming;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Players;

/// <summary>
/// Hosts the bundled TransLib transform state for every player and forwards the hooks the library needs.
/// The API ships as a private library DLL (dllReferences), so its content is never autoloaded: this bridge owns it.
/// </summary>
public class PuppyTransformPlayer : ModPlayer, ITransformHolder
{
    /// <summary>The player's active transform, or null. See the library's ITransformHolder.</summary>
    public Transform? ActiveTransform { get; set; }

    public override void OnRespawn()
    {
        if (ActiveTransform is null)
            return;

        Player.Untransform(fromNet: Main.netMode != NetmodeID.Server);
    }

    public override void UpdateEquips() => ActiveTransform?.Update(Player);

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (ActiveTransform is { } transform)
            TransformRuntime.RunModifyDrawInfo(transform, ref drawInfo);
    }

    // Dogs have paws, not hands: while the dog transform is active, items can't be used.
    // This is a PuppyMod rule, not a TransLib one (TransLib only provides the transform state).
    public override bool CanUseItem(Item item) => ActiveTransform is not DogTransformation;

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        // Join sync is the host mod's job. The library owns the packet format and re-sends the
        // server's own clone with NetSend, so custom transform state reaches the joiner.
        if (Main.netMode != NetmodeID.Server)
            return;

        TransformRuntime.SendJoinSync(Player, toWho, fromWho);
    }
}