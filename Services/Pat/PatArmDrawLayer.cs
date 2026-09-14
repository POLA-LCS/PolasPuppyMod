using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using PuppyMod.Players;

namespace PuppyMod.Services.Pat;

/// <summary>Draws the patting hand reaching toward the puppy while the local player is patting.</summary>
public class PatArmDrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        if (player.whoAmI != Main.myPlayer)
            return;

        var puppy = player.GetModPlayer<PuppyPlayer>();
        if (puppy.PatReachTicks <= 0 || puppy.PatReachTarget < 0)
            return;

        Player target = Main.player[puppy.PatReachTarget];
        if (target == null || !target.active || target.dead)
            return;

        if (!PatService.TryGetPatHandTexture(out Texture2D texture))
            return;

        Vector2 start = player.Center + new Vector2(0f, -14f);
        Vector2 end = target.Center + new Vector2(0f, -target.height * 0.45f);
        Vector2 direction = end - start;
        float length = direction.Length();
        if (length < 4f)
            return;
        direction /= length;

        // Slight bob while patting, like a happy tap tap tap.
        float bob = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20.0) * 2.5f;
        Vector2 handPosition = start + direction * Math.Min(length, 28f) + new Vector2(0f, bob);

        Color color = Lighting.GetColor(handPosition.ToTileCoordinates());
        float rotation = direction.ToRotation() + MathHelper.PiOver2;

        drawInfo.DrawDataCache.Add(new DrawData(
            texture,
            handPosition - Main.screenPosition,
            null,
            color,
            rotation,
            texture.Size() / 2f,
            1f,
            drawInfo.playerEffect,
            0));
    }
}
