using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;

namespace PuppyMod.Content.Items.Ears;

/// <summary>
/// Draws custom puppy ears over the player's head.
/// Head equip textures are 40x1120 frame sheets (20 frames of 40x56) and are sampled by body frame,
/// which these small ear sprites cannot satisfy, so the worn sprite is drawn manually here instead.
/// The registered head equip texture is fully transparent and only reserves the slot; hair stays visible
/// through ArmorIDs.Head.Sets.DrawFullHair.
/// </summary>
public class PuppyEarsDrawLayer : PlayerDrawLayer
{
    /// <summary>Vertical offset from the vanilla head anchor. Negative moves the ears up toward the crown.</summary>
    private const float HeadOffsetY = -18f;

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        return !player.dead && !player.invis && TryGetCustomEars(player, out _);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        // Draw only for the original player image, not afterimages.
        if (drawInfo.shadow != 0f)
            return;

        Player player = drawInfo.drawPlayer;
        if (!TryGetCustomEars(player, out int itemType))
            return;

        Texture2D texture = TextureAssets.Item[itemType].Value;
        Vector2 headAnchor = (drawInfo.Position - Main.screenPosition
            + new Vector2(-player.bodyFrame.Width / 2f + player.width / 2f, player.height - player.bodyFrame.Height + 4f)).Floor()
            + player.headPosition + drawInfo.headVect;
        Vector2 position = (headAnchor + new Vector2(0f, HeadOffsetY)).Floor();

        drawInfo.DrawDataCache.Add(new DrawData(
            texture,
            position,
            null,
            Color.White,
            player.headRotation,
            new Vector2(texture.Width / 2f, texture.Height / 2f),
            1f,
            drawInfo.playerEffect));
    }

    private static bool TryGetCustomEars(Player player, out int itemType)
    {
        itemType = 0;
        int vanityType = 0;

        foreach (PuppyEquipmentEntry entry in player.GetModPlayer<PuppyPlayer>().EquipmentSnapshot.Ears)
        {
            // Vanilla DogEars keeps its own worn sprite.
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
