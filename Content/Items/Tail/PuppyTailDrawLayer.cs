using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;

namespace PuppyMod.Content.Items.Tail;

/// <summary>
/// Draws custom puppy tails on the player's lower back. Custom tails clear their vanilla back slot,
/// so this layer is responsible for their worn sprite in both functional and vanity slots.
/// </summary>
public class PuppyTailDrawLayer : PlayerDrawLayer
{
    /// <summary>Vertical offset from the torso anchor so the tail sits at the lower back instead of the neck.</summary>
    private const float VerticalOffset = 8f;

    /// <summary>Small horizontal offset behind the player, mirrored by facing direction.</summary>
    private const float BackwardOffset = 2f;

    public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Backpacks, PlayerDrawLayers.Wings);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        return !player.dead && !player.invis && !player.mount.Active && TryGetCustomTail(player, out _);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        // Draw only for the original player image, not afterimages.
        if (drawInfo.shadow != 0f)
            return;

        Player player = drawInfo.drawPlayer;
        if (!TryGetCustomTail(player, out int itemType))
            return;

        Texture2D texture = TextureAssets.Item[itemType].Value;
        Vector2 position = drawInfo.Position - Main.screenPosition
            + player.bodyPosition
            + new Vector2(player.width / 2f, player.height - player.bodyFrame.Height / 2f)
            + new Vector2(-player.direction * BackwardOffset, VerticalOffset);

        if (drawInfo.isSitting)
            position.Y -= 2f;

        position = position.Floor();

        drawInfo.DrawDataCache.Add(new DrawData(
            texture,
            position,
            null,
            Color.White,
            player.bodyRotation,
            new Vector2(texture.Width / 2f, texture.Height / 2f),
            1f,
            drawInfo.playerEffect));
    }

    private static bool TryGetCustomTail(Player player, out int itemType)
    {
        itemType = 0;
        int vanityType = 0;

        foreach (PuppyEquipmentEntry entry in player.GetModPlayer<PuppyPlayer>().EquipmentSnapshot.Tails)
        {
            // Vanilla DogTail keeps its own worn sprite.
            if (entry.ItemType < ItemID.Count)
                continue;

            if (entry.IsFunctional)
            {
                itemType = entry.ItemType;
                return true;
            }

            if (vanityType == 0)
                vanityType = entry.ItemType;
        }

        itemType = vanityType;
        return itemType != 0;
    }
}
