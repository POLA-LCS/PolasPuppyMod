using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TransformAPI.Core.Morphing;
using ReLogic.Content;
using SpreadsheetSplit;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;
using PuppyMod.Players;
using TransformAPIMod = TransformAPI.TransformAPI;

namespace PuppyMod.Content.Transformations;

public class DogTransformationMorph : Morph
{
    /// <summary>Accumulated horizontal speed needed to advance one running frame.</summary>
    private const float MovementPerFrame = 7f;

    /// <summary>Ticks each standing-still frame is shown.</summary>
    private const float StandTicksPerFrame = 10f;

    /// <summary>Ticks each jump frame is shown.</summary>
    private const float JumpTicksPerFrame = 8f;

    /// <summary>Ticks each falling frame is shown.</summary>
    private const float FallTicksPerFrame = 8f;

    /// <summary>Ticks each emote frame is shown.</summary>
    private const float EmoteTicksPerFrame = 10f;

    /// <summary>Ticks each scratch frame is shown when not fast.</summary>
    private const float NormalScratchTicksPerFrame = 8.5f;

    /// <summary>Ticks each scratch frame is shown when fast (50% chance).</summary>
    private const float FastScratchTicksPerFrame = 5.5f;

    /// <summary>Cap on how fast the animation can play, so sprinting doesn't blur the frames.</summary>
    private const float MaxAnimationSpeed = 3f;

    /// <summary>Below this horizontal speed the dog is considered standing still.</summary>
    private const float MoveSpeedThreshold = 0.05f;

    /// <summary>Downward speed needed before the falling animation takes over, so the jump apex has a moment of suspension.</summary>
    private const float FallVelocityThreshold = 0.2f;

    /// <summary>Height of the player's hitbox while transformed, in tiles.</summary>
    private const float HitboxHeightTiles = 1.7f;

    /// <summary>Extra width added to the player's hitbox while transformed, in tiles.</summary>
    private const float HitboxExtraWidthTiles = 0.2f;

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
    private float _hitboxOffsetX;

    private AnimationPlayer _standing;
    private AnimationPlayer _moving;
    private AnimationPlayer _jumping;
    private AnimationPlayer _falling;
    private AnimationPlayer _bendCycle;
    private AnimationPlayer _scratchCycle;
    private bool _scratchIsFast;

    /// <summary>Synced breed skin; local owner's config is copied on morph and propagated via NetSend/NetRecieve.</summary>
    public DogTransformationSkin Skin { get; private set; } = DogTransformationSkin.Beagle;

    public override bool HideDefaultPlayer => true;

    public override bool CanUseItem(Player player, Item item) => false;

    public override bool ModifyHitbox(Player player, out Point16 size)
    {
        int width = (int)MathF.Round(Player.defaultWidth + HitboxExtraWidthTiles * 16f);
        int height = (int)MathF.Round(HitboxHeightTiles * 16f);
        size = new Point16(width, height);
        return true;
    }

    /// <summary>Keeps the widened hitbox clear of obstacles without accumulating movement across resizes.</summary>
    public void ApplyHitboxOffset(Player player)
    {
        float extra = player.width - Player.defaultWidth;
        if (extra <= 0f)
        {
            _hitboxOffsetX = 0f;
            return;
        }

        // Recover the game's own position before evaluating the offset again.
        player.position.X -= _hitboxOffsetX;

        float offset = ChooseHitboxOffset(player);
        player.position.X += offset;
        _hitboxOffsetX = offset;
    }

    private float ChooseHitboxOffset(Player player)
    {
        // Prefer keeping the current offset so ordinary walking never shifts the player.
        if (IsHitboxClear(player, _hitboxOffsetX))
            return _hitboxOffsetX;

        float extra = player.width - Player.defaultWidth;
        float centered = -extra / 2f;
        if (IsHitboxClear(player, centered))
            return centered;

        float rightAnchored = -extra;
        if (IsHitboxClear(player, rightAnchored))
            return rightAnchored;

        return 0f;
    }

    private static bool IsHitboxClear(Player player, float offset)
    {
        Vector2 position = player.position + new Vector2(offset, 0f);
        return !Collision.SolidCollision(position, player.width, player.height);
    }

    public override void OnMorph(Player player)
    {
        EnsureAnimationPlayers();

        // Capture owner's chosen breed for sync. Remote clients keep the value received via NetRecieve.
        if (player.whoAmI == Main.myPlayer)
        {
            try
            {
                Skin = ModContent.GetInstance<PuppyModClientConfig>().TransformationSkin;
            }
            catch
            {
                // Fallback to current value if config not available (e.g., dedicated server)
            }
        }

        SpawnPuff(player);
    }

    /// <summary>
    /// Creates the shared animation players once. Join sync runs <see cref="NetRecieve(BinaryReader)"/> on a fresh
    /// clone before <see cref="OnMorph(Player)"/>, so the cycle players must already exist for <c>Seek</c> to work.
    /// </summary>
    private void EnsureAnimationPlayers()
    {
        if (_standing is not null)
            return;

        _standing = DogAnimations.Sheet.PlayAnimation(DogAnimations.Standing);
        _moving = DogAnimations.Sheet.PlayAnimation(DogAnimations.Moving);
        _jumping = DogAnimations.Sheet.PlayAnimation(DogAnimations.Jumping);
        _falling = DogAnimations.Sheet.PlayAnimation(DogAnimations.Falling);
        _bendCycle = DogAnimations.Sheet.PlayAnimation(DogAnimations.BendingCycle);
        _scratchCycle = DogAnimations.Sheet.PlayAnimation(DogAnimations.ScratchingCycle);
    }

    public override void OnUnmorph(Player player)
    {
        player.position.X -= _hitboxOffsetX;
        _hitboxOffsetX = 0f;
        SpawnPuff(player);
    }

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
        _scratchIsFast = emote == DogEmote.Scratch && Main.rand.NextBool();
        SendEmoteUpdate(player);
    }

    private static void SendEmoteUpdate(Player player)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            TransformAPIMod.SendUpdateMorph(player);
    }

    /// <summary>Whether the dog can start an emote right now - emotes only play while grounded and standing still.</summary>
    public bool CanEmote(Player player) => player.velocity.Y == 0f && Math.Abs(player.velocity.X) <= MoveSpeedThreshold;

    public override void Update(Player player)
    {
        // Keep skin in sync if local owner changes config while morphed.
        if (player.whoAmI == Main.myPlayer)
        {
            try
            {
                DogTransformationSkin configSkin = ModContent.GetInstance<PuppyModClientConfig>().TransformationSkin;
                if (configSkin != Skin)
                {
                    Skin = configSkin;
                    TransformAPIMod.SendUpdateMorph(player);
                }
            }
            catch
            {
                // Ignore config lookup failures on server
            }
        }

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

                if (_fallProgress >= FallTicksPerFrame)
                {
                    _fallProgress = 0f;
                    _falling.Advance();
                }
            }
            else
            {
                _jumpProgress += 1f;
                _fallProgress = 0f;

                if (_jumpProgress >= JumpTicksPerFrame)
                {
                    _jumpProgress = 0f;
                    _jumping.Advance();
                }
            }

            return;
        }

        _jumpProgress = 0f;
        _fallProgress = 0f;

        if (moving)
        {
            _runProgress += Math.Min(Math.Abs(player.velocity.X), MaxAnimationSpeed);
            while (_runProgress >= MovementPerFrame)
            {
                _runProgress -= MovementPerFrame;
                _moving.Advance();
            }
        }
        else
        {
            // Being petted makes the tail wag twice as fast.
            _standProgress += player.GetModPlayer<PuppyPlayer>().PatWagTicks > 0 ? 2f : 1f;
            if (_standProgress >= StandTicksPerFrame)
            {
                _standProgress = 0f;
                _standing.Advance();
            }
        }
    }

    private void UpdateEmote(Player player)
    {
        _emoteProgress += 1f;

        float ticksPerFrame = _emote == DogEmote.Scratch
            ? (_scratchIsFast ? FastScratchTicksPerFrame : NormalScratchTicksPerFrame)
            : EmoteTicksPerFrame;
        if (_emoteProgress < ticksPerFrame)
            return;

        _emoteProgress = 0f;

        switch (_emotePhase)
        {
            case EmotePhase.Start:
                _emotePhase = EmotePhase.Cycle;
                GetCyclePlayer(_emote).Reset();
                break;
            case EmotePhase.Cycle:
                AnimationPlayer cycle = GetCyclePlayer(_emote);
                cycle.Advance();
                if (cycle.CurrentIndex == 0)
                {
                    if (_emoteHeld)
                        SendEmoteUpdate(player);
                    else
                        _emotePhase = EmotePhase.End;
                }
                break;
            case EmotePhase.End:
                _emote = DogEmote.None;
                _emotePhase = EmotePhase.None;
                break;
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((byte)_emote);
        writer.Write((byte)_emotePhase);
        writer.Write((short)_emoteProgress);
        writer.Write((byte)GetCycleIndex());
        writer.Write((byte)Skin);
        writer.Write(_scratchIsFast);
    }

    public override void NetRecieve(BinaryReader reader)
    {
        // Join sync delivers NetRecieve on a fresh clone before OnMorph, so make sure the cycle players exist.
        EnsureAnimationPlayers();

        _emote = (DogEmote)reader.ReadByte();
        _emotePhase = (EmotePhase)reader.ReadByte();
        _emoteProgress = reader.ReadInt16();
        byte cycleIndex = reader.ReadByte();

        if (_emote != DogEmote.None)
            GetCyclePlayer(_emote).Seek(cycleIndex);

        // Backward-compatible: skin and fast flag appended at end.
        if (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            DogTransformationSkin received = (DogTransformationSkin)reader.ReadByte();
            if (Enum.IsDefined(typeof(DogTransformationSkin), received))
                Skin = received;
            else
                Skin = DogTransformationSkin.Beagle;
        }

        if (reader.BaseStream.Position < reader.BaseStream.Length)
            _scratchIsFast = reader.ReadBoolean();
    }

    private int GetCycleIndex() => _emote == DogEmote.None ? 0 : GetCyclePlayer(_emote).CurrentIndex;

    private AnimationPlayer GetCyclePlayer(DogEmote emote) => emote == DogEmote.Bend ? _bendCycle : _scratchCycle;

    private static readonly Dictionary<Texture2D, float[]> FrameAlignment = [];

    /// <summary>Per-frame horizontal offsets that align each frame's front edge, so the body stays put while the tail and legs animate.</summary>
    private static float[] GetFrameAlignment(Texture2D texture, int frameCount)
    {
        if (FrameAlignment.TryGetValue(texture, out float[] cached))
            return cached;

        Color[] pixels = new Color[texture.Width * texture.Height];
        texture.GetData(pixels);

        int[] leftEdges = new int[frameCount];
        int anchor = int.MaxValue;

        for (int frame = 0; frame < frameCount; frame++)
        {
            int minX = int.MaxValue;
            int startY = frame * DogAnimations.SpriteHeight;
            int endY = Math.Min(startY + DogAnimations.SpriteHeight, texture.Height);

            for (int y = startY; y < endY; y++)
            {
                int row = y * texture.Width;
                for (int x = 0; x < texture.Width; x++)
                {
                    if (pixels[row + x].A == 0)
                        continue;
                    if (x < minX)
                        minX = x;
                }
            }

            leftEdges[frame] = minX;
            if (minX < anchor)
                anchor = minX;
        }

        float[] offsets = new float[frameCount];
        for (int frame = 0; frame < frameCount; frame++)
        {
            if (leftEdges[frame] != int.MaxValue)
                offsets[frame] = anchor - leftEdges[frame];
        }

        FrameAlignment[texture] = offsets;
        return offsets;
    }

    public override void SetDrawLayers(List<DrawData> oldDrawData, ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        DogTransformationSkin skin = Skin;
        if (!Enum.IsDefined(typeof(DogTransformationSkin), skin))
        {
            try
            {
                skin = ModContent.GetInstance<PuppyModClientConfig>().TransformationSkin;
            }
            catch
            {
                skin = DogTransformationSkin.Beagle;
            }
        }
        Texture2D texture = ModContent.Request<Texture2D>(
            AssetUtils.GetTransformationTexturePath(skin.ToString()),
            AssetRequestMode.ImmediateLoad).Value;

        SpriteSheet sheet = DogAnimations.Sheet;
        int spriteIndex = GetSpriteIndex(player) % sheet.Count;
        SpriteBounds bounds = sheet.GetBounds(spriteIndex);
        Rectangle source = new(bounds.X, bounds.Y, bounds.Width, bounds.Height);

        Vector2 position = (player.Bottom - Main.screenPosition + new Vector2(0f, player.gfxOffY)).Floor();
        SpriteEffects effects = drawInfo.playerEffect ^ SpriteEffects.FlipHorizontally;

        float alignment = GetFrameAlignment(texture, sheet.Count)[spriteIndex];
        if ((effects & SpriteEffects.FlipHorizontally) != 0)
            alignment = -alignment;
        position.X += alignment;

        Vector2 origin = new(DogAnimations.SpriteWidth / 2f, DogAnimations.SpriteBaseline);
        Color color = Lighting.GetColor(player.Center.ToTileCoordinates());

        drawInfo.DrawDataCache.Add(new DrawData(texture, position, source, color, 0f, origin, 1f, effects, 0));
    }

    private int GetSpriteIndex(Player player)
    {
        if (player.velocity.Y > FallVelocityThreshold)
            return _falling.Current.AbsoluteIndex;

        if (player.velocity.Y != 0f)
            return _jumping.Current.AbsoluteIndex;

        if (_emote != DogEmote.None)
            return GetEmoteSpriteIndex();

        if (Math.Abs(player.velocity.X) > MoveSpeedThreshold)
            return _moving.Current.AbsoluteIndex;

        return _standing.Current.AbsoluteIndex;
    }

    private int GetEmoteSpriteIndex()
    {
        Range whole = DogAnimations.Sheet.Animations[GetEmoteName(_emote)];

        return _emotePhase switch
        {
            EmotePhase.Start => whole.Start.Value,
            EmotePhase.Cycle => GetCyclePlayer(_emote).Current.AbsoluteIndex,
            EmotePhase.End => whole.End.Value - 1,
            _ => whole.Start.Value
        };
    }

    private static string GetEmoteName(DogEmote emote) => emote == DogEmote.Bend ? DogAnimations.Bending : DogAnimations.Scratching;
}
