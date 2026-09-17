using System;
using Microsoft.Xna.Framework;
using Terraria;
using TransformAPI.Core;
using PuppyMod.Players;

namespace PuppyMod.Content.Transformations;

/// <summary>Shared start/toggle logic for the dog transformation used by the Quick Mount path and the dedicated keybind.</summary>
public static class DogTransformController
{
    /// <summary>Vertical clearance in tiles needed to turn back into the full-size player.</summary>
    public const float UntransformClearanceTiles = 2.7f;

    /// <summary>Whether the player has enough headroom for the full-size hitbox.</summary>
    public static bool CanReturnToNormalSize(Player player)
    {
        int height = (int)MathF.Round(UntransformClearanceTiles * 16f);
        Vector2 position = new(player.Bottom.X - Player.defaultWidth / 2f, player.Bottom.Y - height);
        return !Collision.SolidCollision(position, Player.defaultWidth, height);
    }

    /// <summary>Whether a mount item is equipped in the mount slot.</summary>
    public static bool HasMountEquipped(Player player) => !player.miscEquips[3].IsAir;

    /// <summary>Applies the dog transformation, dismounting first so the dog hitbox and drawing can take over.</summary>
    public static void Start(Player player)
    {
        if (player.GetTransform<DogTransformation>() is not null)
            return;

        if (player.mount.Active)
            player.mount.Dismount(player);

        player.SetTransform(new DogTransformation());
    }

    /// <summary>Dedicated keybind toggle: untransforms when transformed (with headroom), otherwise transforms when the puppy set is active, ignoring any equipped mount.</summary>
    public static void Toggle(Player player)
    {
        if (player.whoAmI != Main.myPlayer || player.dead)
            return;

        if (player.GetTransform<DogTransformation>() is not null)
        {
            if (CanReturnToNormalSize(player))
                player.Untransform();

            return;
        }

        if (player.GetModPlayer<PuppyPlayer>().IsPuppy)
            Start(player);
    }
}
