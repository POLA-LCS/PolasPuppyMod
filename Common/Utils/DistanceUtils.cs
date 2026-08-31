using Microsoft.Xna.Framework;
using Terraria;

namespace PuppyMod.Common.Utils;

public static class DistanceUtils
{
    public static bool WithinTiles(Player a, Player b, float tiles) => WithinPixels(a.Center, b.Center, tiles * 16f);
    public static bool WithinPixels(Vector2 a, Vector2 b, float pixels) => Vector2.DistanceSquared(a, b) <= pixels * pixels;
    public static float TilesToPixels(float tiles) => tiles * 16f;
}
