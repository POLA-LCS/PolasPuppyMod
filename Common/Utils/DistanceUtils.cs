using Microsoft.Xna.Framework;
using Terraria;

namespace PuppyMod.Common.Utils;

public static class DistanceUtils
{
    /// <summary>Terraria tile size in pixels (16px per tile) – kept as named const for audit.</summary>
    public const float TilePixels = 16f;

    public static bool WithinTiles(Player a, Player b, float tiles) => WithinPixels(a.Center, b.Center, tiles * TilePixels);

    public static bool WithinPixels(Vector2 a, Vector2 b, float pixels) => Vector2.DistanceSquared(a, b) <= pixels * pixels;

    public static float TilesToPixels(float tiles) => tiles * TilePixels;
}
