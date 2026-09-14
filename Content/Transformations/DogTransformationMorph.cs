using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MorphAPI.Core.Morphing;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;
using MorphAPIMod = MorphAPI.MorphAPI;

namespace PuppyMod.Content.Transformations;

public class DogTransformationMorph : Morph
{
    /// <summary>Vertical distance between frames in the breed sheets.</summary>
    private const int FrameHeight = 38;

    /// <summary>Distance from the top of a frame to the dog's feet.</summary>
    private const int FrameBaseline = 36;

    // Sheet groups as 0-based frame indexes: 0-8 stand still, 9 jump/fall, 10-17 running, 18-23 bend idle, 24-27 scratch.
    private const int StandStart = 0;
    private const int StandFrameCount = 9;
    private const int JumpFrame = 9;
    private const int RunStart = 10;
    private const int RunFrameCount = 8;
    private const int BendStart = 18;
    private const int BendFrameCount = 6;
    private const int ScratchStart = 24;
    private const int ScratchFrameCount = 4;

    /// <summary>Accumulated horizontal speed needed to advance one running frame.</summary>
    private const float MovementPerFrame = 7f;

    /// <summary>Ticks each standing-still frame is shown.</summary>
    private const float StandTicksPerFrame = 10f;

    /// <summary>Ticks each emote frame is shown.</summary>
    private const float EmoteTicksPerFrame = 10f;

    /// <summary>Cap on how fast the animation can play, so sprinting doesn't blur the frames.</summary>
    private const float MaxAnimationSpeed = 3f;

    /// <summary>Below this horizontal speed the dog is considered standing still.</summary>
    private const float MoveSpeedThreshold = 0.05f;

    /// <summary>Dust spawned for the transformation puff.</summary>
    private const int PuffDustCount = 25;

    /// <summary>Maximum outward speed of the transformation puff dust.</summary>
    private const float PuffSpeed = 2.2f;

    private float _runProgress;
    private float _standProgress;
    private float _emoteProgress;
    private DogEmote _emote;

    public override bool HideDefaultPlayer => true;

    public override bool CanUseItem(Player player, Item item) => false;

    public override void OnMorph(Player player) => SpawnPuff(player);

    public override void OnUnmorph(Player player) => SpawnPuff(player);

    private static void SpawnPuff(Player player)
    {
        if (Main.dedServ)
            return;

        for (int i = 0; i < PuffDustCount; i++)
        {
            Vector2 offset = new(
                Main.rand.NextFloat(-player.width, player.width),
                Main.rand.NextFloat(-player.height * 0.5f, player.height * 0.5f));

            Dust dust = Dust.NewDustPerfect(
                player.Center + offset,
                DustID.Cloud,
                Main.rand.NextVector2Circular(PuffSpeed, PuffSpeed),
                0,
                default,
                Main.rand.NextFloat(1.2f, 1.8f));

            dust.noGravity = true;
            dust.velocity.Y -= 0.4f;
        }
    }

    /// <summary>Starts a one-shot emote animation and syncs it in multiplayer.</summary>
    public void PlayEmote(Player player, DogEmote emote)
    {
        if (emote == DogEmote.None)
            return;

        _emote = emote;
        _emoteProgress = 0f;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            MorphAPIMod.SendUpdateMorph(player);
    }

    public override void Update(Player player)
    {
        bool moving = Math.Abs(player.velocity.X) > MoveSpeedThreshold;
        bool airborne = player.velocity.Y != 0f;

        if (_emote != DogEmote.None)
        {
            if (moving || airborne)
            {
                _emote = DogEmote.None;
                _emoteProgress = 0f;
            }
            else
            {
                _emoteProgress += 1f;
                if (_emoteProgress >= GetEmoteFrameCount(_emote) * EmoteTicksPerFrame)
                {
                    _emote = DogEmote.None;
                    _emoteProgress = 0f;
                }

                return;
            }
        }

        if (airborne)
            return;

        if (moving)
            _runProgress += Math.Min(Math.Abs(player.velocity.X), MaxAnimationSpeed);
        else
            _standProgress += 1f;
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((byte)_emote);
        writer.Write((short)_emoteProgress);
    }

    public override void NetRecieve(BinaryReader reader)
    {
        _emote = (DogEmote)reader.ReadByte();
        _emoteProgress = reader.ReadInt16();
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
        SpriteEffects effects = drawInfo.playerEffect ^ SpriteEffects.FlipHorizontally;

        drawInfo.DrawDataCache.Add(new DrawData(texture, position.Floor(), source, color, 0f, origin, 1f, effects, 0));
    }

    private int GetFrame(Player player)
    {
        if (player.velocity.Y != 0f)
            return JumpFrame;

        if (_emote == DogEmote.Bend)
            return BendStart + (int)(_emoteProgress / EmoteTicksPerFrame) % BendFrameCount;

        if (_emote == DogEmote.Scratch)
            return ScratchStart + (int)(_emoteProgress / EmoteTicksPerFrame) % ScratchFrameCount;

        if (Math.Abs(player.velocity.X) > MoveSpeedThreshold)
            return RunStart + (int)(_runProgress / MovementPerFrame) % RunFrameCount;

        return StandStart + (int)(_standProgress / StandTicksPerFrame) % StandFrameCount;
    }

    private static int GetEmoteFrameCount(DogEmote emote) => emote switch
    {
        DogEmote.Bend => BendFrameCount,
        DogEmote.Scratch => ScratchFrameCount,
        _ => 0
    };
}
