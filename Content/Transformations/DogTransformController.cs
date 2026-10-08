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
    /// Whether vanilla Quick Mount would fire its minecart hook right now, which it does when a
    /// Minecart Track sits inside its search rect around the player.
    /// </summary>
    /// <remarks>
    /// The Quick Mount key doubles as the transformation key, but the transformation must not swallow
    /// a rail request. With no mount item, vanilla falls through to its minecart path, so the check is
    /// made before transforming and vanilla is allowed to handle the press when a track is in range.
    /// The search mirrors vanilla: the same clamped rect, the same inclusive tile span, and the same
    /// acceptance rule where the first track found is taken unconditionally and later tracks only
    /// replace it when they are both closer and in line of sight.
    /// </remarks>
    public static bool HasMinecartTrackInRange(Player player)
    {
        int startX = (int)(player.position.X / 16f) - Player.tileRangeX + 1;
        int endX = (int)((player.position.X + player.width) / 16f) + Player.tileRangeX - 1;
        int startY = (int)(player.position.Y / 16f) - Player.tileRangeY + 1;
        int endY = (int)((player.position.Y + player.height) / 16f) + Player.tileRangeY - 2;

        // Vanilla clamps the rect before reading tiles, so its search never touches the world border.
        startX = Utils.Clamp(startX, 10, Main.maxTilesX - 10);
        endX = Utils.Clamp(endX, 10, Main.maxTilesX - 10);
        startY = Utils.Clamp(startY, 10, Main.maxTilesY - 10);
        endY = Utils.Clamp(endY, 10, Main.maxTilesY - 10);

        // GetTilesIn covers the pixel rect up to (endX + 1, endY + 1) and iterates inclusively.
        Point? target = null;
        for (int x = startX; x <= endX + 1; x++)
        {
            for (int y = startY; y <= endY + 1; y++)
            {
                Tile tile = Framing.GetTileSafely(x, y);
                if (!tile.HasTile || tile.TileType != TileID.MinecartTrack)
                    continue;

                Vector2 tileCenter = new Vector2(x, y) * 16f + new Vector2(8f);
                if (!target.HasValue ||
                    (Vector2.Distance(player.Center, tileCenter) < Vector2.Distance(player.Center, target.Value.ToVector2() * 16f + new Vector2(8f))
                     && Collision.CanHitLine(player.Center, 0, 0, tileCenter, 0, 0)))
                {
                    target = new Point(x, y);
                }
            }
        }

        return target.HasValue;
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
