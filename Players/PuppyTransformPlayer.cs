#nullable enable
using TransformLib.Core;
using TransformLib.Core.Transforming;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Players;

/// <summary>
/// Hosts the bundled TransformLib transform state for every player and forwards the hooks the library needs.
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

    public override bool CanUseItem(Item item) => ActiveTransform is not { } transform || transform.CanUseItem(Player, item);

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (Main.netMode != NetmodeID.Server || ActiveTransform is not { } transform)
            return;

        TransformRuntime.SendSetTransform(transform, Player, toWho, fromWho);
    }
}
