using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TransLib.Core;
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

    /// <summary>
    /// Whether vanilla Quick Mount would put the player in a minecart right now, which it does when a
    /// Minecart Track sits in the same tile range and has line of sight to the player.
    /// </summary>
    /// <remarks>
    /// The Quick Mount key doubles as the transformation key, but the transformation must not swallow
    /// a rail request. With no mount item, vanilla falls through to its minecart path, so the check is
    /// made before transforming and vanilla is allowed to handle the press when a track is reachable.
    /// </remarks>
    public static bool HasReachableMinecartTrack(Player player)
    {
        int startX = (int)(player.position.X / 16f) - Player.tileRangeX + 1;
        int endX = (int)((player.position.X + player.width) / 16f) + Player.tileRangeX - 1;
        int startY = (int)(player.position.Y / 16f) - Player.tileRangeY + 1;
        int endY = (int)((player.position.Y + player.height) / 16f) + Player.tileRangeY - 2;

        bool found = false;
        float closest = float.MaxValue;
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Tile tile = Framing.GetTileSafely(x, y);
                if (!tile.HasTile || tile.TileType != TileID.MinecartTrack)
                    continue;

                Vector2 tileCenter = new Vector2(x, y) * 16f + new Vector2(8f);
                float distance = Vector2.Distance(player.Center, tileCenter);
                if (distance >= closest)
                    continue;

                // Vanilla only takes the nearest visible track, so a blocked nearest tile must not
                // hide a visible one further away.
                if (!Collision.CanHitLine(player.Center, 0, 0, tileCenter, 0, 0))
                    continue;

                closest = distance;
                found = true;
            }
        }

        return found;
    }

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
