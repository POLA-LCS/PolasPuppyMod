using Microsoft.Xna.Framework;
using Terraria;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;

namespace PuppyMod.Services.PuppySets;

/// <summary>
/// Applies the Shiny puppy set's ore sight. The effect belongs to the pair bonus, so it only
/// exists while the matching ShinyEars + ShinyTail pair is the selected set, and its radius
/// scales with the pair's placement state (Costume 5 / Furry 10 / Therian 15).
/// </summary>
public static class PuppySpelunkerService
{
    private const float TileSize = 16f;
    private const float HalfTile = 8f;
    private const float LightIntensity = 0.25f;

    public static void Apply(Player player)
    {
        if (Main.dedServ || player == null || !player.active || player.dead)
            return;

        PuppyEquipmentResolution resolution = player.GetModPlayer<PuppyPlayer>().EquipmentResolution;
        if (!resolution.TryGetPairBonus(out PuppyPairBonusDefinition pairBonus) || !pairBonus.Spelunker.HasValue)
            return;

        HighlightSpelunkerTiles(player, pairBonus.Spelunker.Value.GetRadius(resolution.SelectedPlacement));
    }

    private static void HighlightSpelunkerTiles(Player player, int radius)
    {
        Vector3 color = new(LightIntensity, LightIntensity * 0.85f, LightIntensity * 0.35f);
        Point center = player.Center.ToTileCoordinates();
        for (int x = center.X - radius; x <= center.X + radius; x++)
        for (int y = center.Y - radius; y <= center.Y + radius; y++)
        {
            if (!WorldGen.InWorld(x, y)) continue;
            Tile tile = Main.tile[x, y];
            if (!tile.HasTile || tile.IsActuated) continue;
            if (Main.tileSpelunker[tile.TileType])
                Lighting.AddLight(new Vector2(x * TileSize + HalfTile, y * TileSize + HalfTile), color);
        }
    }
}
