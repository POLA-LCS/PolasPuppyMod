using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MorphAPI.Core.Morphing;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;

namespace PuppyMod.Content.Transformations;

public class DogTransformationMorph : Morph
{
    /// <summary>Vertical distance between frames in the breed sheets.</summary>
    private const int FrameHeight = 38;

    /// <summary>Distance from the top of a frame to the dog's feet.</summary>
    private const int FrameBaseline = 36;

    /// <summary>Frame shown while standing still.</summary>
    private const int IdleFrame = 0;

    /// <summary>Accumulated horizontal speed needed to advance one animation frame.</summary>
    private const float MovementPerFrame = 6f;

    /// <summary>Cap on how fast the animation can play, so sprinting doesn't blur the frames.</summary>
    private const float MaxAnimationSpeed = 3f;

    /// <summary>Below this horizontal speed the dog is considered standing still.</summary>
    private const float MoveSpeedThreshold = 0.05f;

    private float _animationProgress;

    public override bool HideDefaultPlayer => true;

    public override bool BlockMounts => true;

    public override void Update(Player player)
    {
        float speed = Math.Abs(player.velocity.X);
        if (speed <= MoveSpeedThreshold)
        {
            _animationProgress = 0f;
            return;
        }

        _animationProgress += Math.Min(speed, MaxAnimationSpeed);
    }

    public override void SetDrawLayers(List<DrawData> oldDrawData, ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        DogTransformationSkin skin = ModContent.GetInstance<PuppyModClientConfig>().TransformationSkin;
        Texture2D texture = ModContent.Request<Texture2D>(
            AssetUtils.GetTransformationTexturePath(skin.ToString()),
            AssetRequestMode.ImmediateLoad).Value;

        int frameCount = Math.Max(1, texture.Height / FrameHeight);
        int frame = GetFrame(player) % frameCount;
        Rectangle source = new(0, frame * FrameHeight, texture.Width, FrameHeight);
        Vector2 position = player.Bottom - Main.screenPosition + new Vector2(0f, player.gfxOffY);
        Vector2 origin = new(texture.Width / 2f, FrameBaseline);
        Color color = Lighting.GetColor(player.Center.ToTileCoordinates());

        drawInfo.DrawDataCache.Add(new DrawData(texture, position.Floor(), source, color, 0f, origin, 1f, drawInfo.playerEffect, 0));
    }

    private int GetFrame(Player player)
    {
        if (Math.Abs(player.velocity.X) <= MoveSpeedThreshold)
            return IdleFrame;

        return (int)(_animationProgress / MovementPerFrame);
    }
}
