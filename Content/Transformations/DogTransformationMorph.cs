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

    // Sheet groups as 0-based frame indexes: 0-7 stand still, 8 jump/fall, 9-16 running, 17-22 bend, 23-27 scratch.
    // Emotes play their first frame once, cycle the middle frames, and play their last frame once when the key is released.
    private const int StandStart = 0;
    private const int StandFrameCount = 8;
    private const int JumpFrameStart = 8;
    private const int JumpFrameCount = 1;
    private const int FallFrameStart = 8;
    private const int FallFrameCount = 1;
    private const int RunStart = 9;
    private const int RunFrameCount = 8;
    private const int BendStart = 17;
    private const int BendFrameCount = 6;
    private const int ScratchStart = 23;
    private const int ScratchFrameCount = 5;

    /// <summary>Accumulated horizontal speed needed to advance one running frame.</summary>
    private const float MovementPerFrame = 7f;

    /// <summary>Ticks each standing-still frame is shown.</summary>
    private const float StandTicksPerFrame = 10f;

    /// <summary>Ticks each jump/fall frame is shown.</summary>
    private const float JumpTicksPerFrame = 8f;

    /// <summary>Ticks each falling frame is shown.</summary>
    private const float FallTicksPerFrame = 8f;

    /// <summary>Ticks each emote frame is shown.</summary>
    private const float EmoteTicksPerFrame = 10f;

    /// <summary>Cap on how fast the animation can play, so sprinting doesn't blur the frames.</summary>
    private const float MaxAnimationSpeed = 3f;

    /// <summary>Below this horizontal speed the dog is considered standing still.</summary>
    private const float MoveSpeedThreshold = 0.05f;

    /// <summary>Downward speed needed before the falling animation takes over, so the jump apex has a moment of suspension.</summary>
    private const float FallVelocityThreshold = 0.2f;

    /// <summary>Height of the player's hitbox while transformed, in tiles.</summary>
    private const float HitboxHeightTiles = 1.8f;

    /// <summary>Dust spawned for the transformation puff.</summary>
    private const int PuffDustCount = 25;

    /// <summary>Maximum outward speed of the transformation puff dust.</summary>
    private const float PuffSpeed = 2.2f;

    private enum EmotePhase : byte
    {
        None,
        Start,
        Cycle,
        End
    }

    private float _runProgress;
    private float _standProgress;
    private float _jumpProgress;
    private float _fallProgress;
    private float _emoteProgress;
    private DogEmote _emote;
    private EmotePhase _emotePhase;
    private bool _emoteHeld;

    public override bool HideDefaultPlayer => true;

    public override bool CanUseItem(Player player, Item item) => false;

    public override bool ModifyHitbox(Player player, out Point16 size)
    {
        size = new Point16(Player.defaultWidth, (int)MathF.Round(HitboxHeightTiles * 16f));
        return true;
    }

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

    /// <summary>Feeds the emote key state - starts the emote on press and repeats its cycling frames while held.</summary>
    public void SetEmoteInput(Player player, DogEmote emote)
    {
        if (!CanEmote(player))
            emote = DogEmote.None;

        _emoteHeld = emote != DogEmote.None && _emote == emote;

        if (emote == DogEmote.None || _emote == emote)
            return;

        _emote = emote;
        _emotePhase = EmotePhase.Start;
        _emoteProgress = 0f;
        SendEmoteUpdate(player);
    }

    private static void SendEmoteUpdate(Player player)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            MorphAPIMod.SendUpdateMorph(player);
    }

    /// <summary>Whether the dog can start an emote right now - emotes only play while grounded and standing still.</summary>
    public bool CanEmote(Player player) => player.velocity.Y == 0f && Math.Abs(player.velocity.X) <= MoveSpeedThreshold;

    public override void Update(Player player)
    {
        bool moving = Math.Abs(player.velocity.X) > MoveSpeedThreshold;
        bool airborne = player.velocity.Y != 0f;

        if (_emote != DogEmote.None)
        {
            if (moving || airborne)
            {
                _emote = DogEmote.None;
                _emotePhase = EmotePhase.None;
                _emoteProgress = 0f;
            }
            else
            {
                UpdateEmote(player);
                return;
            }
        }

        if (airborne)
        {
            if (player.velocity.Y > FallVelocityThreshold)
            {
                _fallProgress += 1f;
                _jumpProgress = 0f;
            }
            else
            {
                _jumpProgress += 1f;
                _fallProgress = 0f;
            }

            return;
        }

        _jumpProgress = 0f;
        _fallProgress = 0f;

        if (moving)
            _runProgress += Math.Min(Math.Abs(player.velocity.X), MaxAnimationSpeed);
        else
            _standProgress += 1f;
    }

    private void UpdateEmote(Player player)
    {
        _emoteProgress += 1f;

        if (_emoteProgress < EmoteTicksPerFrame)
            return;

        switch (_emotePhase)
        {
            case EmotePhase.Start:
                _emoteProgress = 0f;
                _emotePhase = EmotePhase.Cycle;
                break;
            case EmotePhase.Cycle:
                if (_emoteProgress < GetEmoteCycleCount(_emote) * EmoteTicksPerFrame)
                    break;

                _emoteProgress = 0f;

                if (_emoteHeld)
                    SendEmoteUpdate(player);
                else
                    _emotePhase = EmotePhase.End;
                break;
            case EmotePhase.End:
                _emote = DogEmote.None;
                _emotePhase = EmotePhase.None;
                _emoteProgress = 0f;
                break;
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((byte)_emote);
        writer.Write((byte)_emotePhase);
        writer.Write((short)_emoteProgress);
    }

    public override void NetRecieve(BinaryReader reader)
    {
        _emote = (DogEmote)reader.ReadByte();
        _emotePhase = (EmotePhase)reader.ReadByte();
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
        if (player.velocity.Y > FallVelocityThreshold)
            return FallFrameStart + (int)(_fallProgress / FallTicksPerFrame) % FallFrameCount;

        if (player.velocity.Y != 0f)
            return JumpFrameStart + (int)(_jumpProgress / JumpTicksPerFrame) % JumpFrameCount;

        if (_emote != DogEmote.None)
            return GetEmoteFrame();

        if (Math.Abs(player.velocity.X) > MoveSpeedThreshold)
            return RunStart + (int)(_runProgress / MovementPerFrame) % RunFrameCount;

        return StandStart + (int)(_standProgress / StandTicksPerFrame) % StandFrameCount;
    }

    private int GetEmoteFrame()
    {
        int start = _emote == DogEmote.Bend ? BendStart : ScratchStart;
        int count = _emote == DogEmote.Bend ? BendFrameCount : ScratchFrameCount;

        return _emotePhase switch
        {
            EmotePhase.Start => start,
            EmotePhase.Cycle => start + 1 + (int)(_emoteProgress / EmoteTicksPerFrame) % (count - 2),
            EmotePhase.End => start + count - 1,
            _ => start
        };
    }

    private static int GetEmoteCycleCount(DogEmote emote) => emote switch
    {
        DogEmote.Bend => BendFrameCount - 2,
        DogEmote.Scratch => ScratchFrameCount - 2,
        _ => 0
    };
}
