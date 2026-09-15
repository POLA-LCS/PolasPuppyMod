using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TransformAPI.Core;
using PetAnyone;
using PuppyMod.Common.PuppySets.Bonuses;
using PuppyMod.Common.PuppySets.Core;
using PuppyMod.Common.PuppySets.Definitions;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Items.Ears;
using PuppyMod.Content.Transformations;
using PuppyMod.Players;
using PuppyMod.Services.Leash;
using PuppyMod.Services.Petting;

namespace PuppyMod;

public enum PuppyPacketType : byte
{
    RequestAttach = 1,
    RequestDetach = 2,
    State = 3,
    Pat = 4, // player petting; wire layout unchanged
    PetNpc = 5, // NPC petting via chat button
    Bark = 6, // RequestBark client->server
    RequestBark = 6,
    BarkBroadcast = 7 // server->all
}

public class PuppyMod : Mod
{
    public override uint ExtraPlayerBuffSlots => 1;

    /// <summary>Public mod-call surface for the generic petting API.</summary>
    public override object Call(params object[] args) => PettingApi.Call(args);

    public override void Load()
    {
        PuppyKeybinds.Load(this);
        On_Player.QuickMount += HandleQuickMount;
        PuppyEquipmentRegistry.RegisterDefaults();
        PuppyPairBonusRegistry.RegisterDefaults();
        PuppyPettingPlugin.Load(this);

        // Animated vanity sheets: 40x1120 equip sheets (20 frames of 40x56) animated by the player's body frame.
        EquipLoader.AddEquipTexture(this, PuppyEquipmentTextures.ShinyEarsSheet, EquipType.Head, name: PuppyEquipmentTextures.ShinySetEarsHead);
        EquipLoader.AddEquipTexture(this, PuppyEquipmentTextures.ReinforcedEarsSheet, EquipType.Head, name: PuppyEquipmentTextures.ReinforcedSetEarsHead);
        EquipLoader.AddEquipTexture(this, PuppyEquipmentTextures.ShinyTailSheet, EquipType.Back, name: PuppyEquipmentTextures.ShinySetTailBack);
        EquipLoader.AddEquipTexture(this, PuppyEquipmentTextures.ReinforcedTailSheet, EquipType.Back, name: PuppyEquipmentTextures.ReinforcedSetTailBack);
    }

    public override void PostSetupContent()
    {
        On_Player.ResizeHitbox += RecenterMorphedHitbox;
    }

    public override void Unload()
    {
        On_Player.ResizeHitbox -= RecenterMorphedHitbox;
        On_Player.QuickMount -= HandleQuickMount;
        PuppyKeybinds.Unload();
        PuppyEquipmentRegistry.Clear();
        PuppyPairBonusRegistry.Clear();
    }

    /// <summary>Vertical clearance in tiles needed to turn back into the full-size player.</summary>
    private const float UntransformClearanceTiles = 2.7f;

    /// <summary>The mount key toggles the puppy transformation; with a mount equipped it detransforms and mounts instead.</summary>
    private static void HandleQuickMount(On_Player.orig_QuickMount orig, Player player)
    {
        if (player.whoAmI != Main.myPlayer)
        {
            orig(player);
            return;
        }

        if (player.GetMorph<DogTransformationMorph>() is not null)
        {
            if (!CanReturnToNormalSize(player))
                return;

            player.Unmorph();

            if (!HasMountEquipped(player))
                return;
        }
        else if (!HasMountEquipped(player) && player.GetModPlayer<PuppyPlayer>().IsPuppy)
        {
            player.SetMorph(new DogTransformationMorph());
            return;
        }

        orig(player);
    }

    /// <summary>Whether the player has enough headroom for the full-size hitbox.</summary>
    private static bool CanReturnToNormalSize(Player player)
    {
        int height = (int)MathF.Round(UntransformClearanceTiles * 16f);
        Vector2 position = new(player.Bottom.X - Player.defaultWidth / 2f, player.Bottom.Y - height);
        return !Collision.SolidCollision(position, Player.defaultWidth, height);
    }

    private static bool HasMountEquipped(Player player) => !player.miscEquips[3].IsAir;

    /// <summary>TransformAPI applies the custom width without recentering, so it can embed into walls; the morph keeps it clear of obstacles.</summary>
    private static void RecenterMorphedHitbox(On_Player.orig_ResizeHitbox orig, Player player)
    {
        orig(player);

        if (player.whoAmI == Main.myPlayer)
            player.GetMorph<DogTransformationMorph>()?.ApplyHitboxOffset(player);
    }

    public void RequestLeashAttach(int targetWho, int leashItemType)
    {
        // Client only; server path uses SetGrabberAuthority.
        Debug.Assert(Main.netMode != NetmodeID.Server, "RequestLeashAttach should not be called on server");
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.RequestAttach);
        packet.Write((byte)targetWho);
        packet.Write(leashItemType);
        packet.Send();
    }

    public void RequestLeashDetach(int targetWho)
    {
        // Client only; server path uses SetGrabberAuthority.
        Debug.Assert(Main.netMode != NetmodeID.Server, "RequestLeashDetach should not be called on server");
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.RequestDetach);
        packet.Write((byte)targetWho);
        packet.Send();
    }

    public void BroadcastLeashState(int ownerWho, int targetWho, int leashItemType, int collarItemType = 0)
    {
        if (Main.netMode != NetmodeID.Server)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.State);
        packet.Write((byte)ownerWho);
        packet.Write((byte)targetWho);
        packet.Write(leashItemType);
        packet.Write(collarItemType);
        packet.Send();
    }

    public void BroadcastLeashDetached(int targetWho)
    {
        if (Main.netMode != NetmodeID.Server)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.State);
        packet.Write(byte.MaxValue);
        packet.Write((byte)targetWho);
        packet.Write(0);
        packet.Write(0);
        packet.Send();
    }

    public void RequestPet(int targetWho)
    {
        // Client only; the server applies the pet.
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.Pat);
        packet.Write((byte)targetWho);
        packet.Send();
    }

    public void BroadcastPet(int patterWho, int targetWho)
    {
        if (Main.netMode != NetmodeID.Server)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.Pat);
        packet.Write((byte)patterWho);
        packet.Write((byte)targetWho);
        packet.Send();
    }

    public void RequestPetNpc(int npcWhoAmI)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.PetNpc);
        packet.Write((byte)npcWhoAmI);
        packet.Send();
    }

    public void BroadcastPetNpc(int patterWho, int npcWhoAmI)
    {
        if (Main.netMode != NetmodeID.Server)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.PetNpc);
        packet.Write((byte)patterWho);
        packet.Write((byte)npcWhoAmI);
        packet.Send();
    }

    // --- Bark networking ---
    /// <summary>Cooldown tracker for server-side bark validation, keyed by emitter whoAmI.</summary>
    private readonly Dictionary<int, int> _serverBarkLastTick = new();

    /// <summary>Client → server: request bark. kind 0=Bark,1=Cry,2=Growl, index = sound index, pitchStyle = BarkPitchStyle byte, pitched = +0.3, pos = world center.</summary>
    public void RequestBark(byte kind, byte index, byte pitchStyle, bool pitched, Vector2 pos)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;
        var packet = GetPacket();
        packet.Write((byte)PuppyPacketType.RequestBark);
        packet.Write(kind);
        packet.Write(index);
        packet.Write(pitchStyle);
        packet.Write(pitched);
        packet.Write(pos.X);
        packet.Write(pos.Y);
        packet.Send();
    }

    /// <summary>Server → all: broadcast validated bark.</summary>
    public void BroadcastBark(byte emitterWho, byte kind, byte index, byte pitchStyle, bool pitched, Vector2 pos)
    {
        if (Main.netMode != NetmodeID.Server)
            return;
        var serverConfig = ModContent.GetInstance<PuppyModServerConfig>();
        if (!serverConfig.BarkEnabled)
            return;
        // Range-limited broadcast: 0 = infinite/global, else tiles*16 radius.
        if (serverConfig.BarkRangeTiles > 0)
        {
            float rangePixels = serverConfig.BarkRangeTiles * 16f;
            float rangeSq = rangePixels * rangePixels;
            Player emitter = IsValidPlayer(emitterWho) ? Main.player[emitterWho] : null;
            Vector2 emitterCenter = emitter != null ? emitter.Center : pos;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player target = Main.player[i];
                if (target == null || !target.active)
                    continue;
                // Emitter always receives for dedup, others within range.
                if (i != emitterWho && Vector2.DistanceSquared(emitterCenter, target.Center) > rangeSq)
                    continue;
                var packet = GetPacket();
                packet.Write((byte)PuppyPacketType.BarkBroadcast);
                packet.Write(emitterWho);
                packet.Write(kind);
                packet.Write(index);
                packet.Write(pitchStyle);
                packet.Write(pitched);
                packet.Write(pos.X);
                packet.Write(pos.Y);
                packet.Send(toClient: i);
            }
        }
        else
        {
            var packet = GetPacket();
            packet.Write((byte)PuppyPacketType.BarkBroadcast);
            packet.Write(emitterWho);
            packet.Write(kind);
            packet.Write(index);
            packet.Write(pitchStyle);
            packet.Write(pitched);
            packet.Write(pos.X);
            packet.Write(pos.Y);
            packet.Send();
        }
    }

    private void HandleServerBark(int whoAmI, BinaryReader reader)
    {
        // Length guard: kind(1)+index(1)+pitchStyle(1)+pitched(1)+pos(8) = 12
        if (reader.BaseStream.Position + 12 > reader.BaseStream.Length)
            return;
        byte kind = reader.ReadByte();
        byte index = reader.ReadByte();
        byte pitchStyleByte = reader.ReadByte();
        bool pitched = reader.ReadBoolean();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        Vector2 pos = new(x, y);

        if (!IsValidPlayer(whoAmI))
            return;
        Player emitter = Main.player[whoAmI];
        if (emitter.dead)
            return;
        var pp = emitter.GetModPlayer<PuppyPlayer>();
        if (pp == null || !pp.IsPuppy)
            return;
        var serverConfig = ModContent.GetInstance<PuppyModServerConfig>();
        if (!serverConfig.BarkEnabled)
            return;
        if (kind > 2)
            return;
        if (index >= 64)
            return;
        if (pitchStyleByte > (byte)BarkPitchStyle.Squeak)
            return;
        // Pos distance sanity: must be within 500 tiles (~8000 px) of emitter.
        if (Vector2.DistanceSquared(emitter.Center, pos) > 8000f * 8000f)
            return;
        // Per-player cooldown (server dict)
        int now = (int)Main.GameUpdateCount;
        if (_serverBarkLastTick.TryGetValue(whoAmI, out int lastTick) && now - lastTick < PuppyPlayer.BarkCooldownTicks)
            return;

        BroadcastBark((byte)whoAmI, kind, index, pitchStyleByte, pitched, pos);
        _serverBarkLastTick[whoAmI] = now;
        // Also set emitter's PuppyPlayer server cooldown to keep instance in sync for dedicated server ticks.
        pp.SetServerBarkCooldown();
    }

    private void HandleClientBarkBroadcast(BinaryReader reader)
    {
        // Length guard: emitter(1)+kind(1)+index(1)+pitchStyle(1)+pitched(1)+pos(8)=13
        if (reader.BaseStream.Position + 13 > reader.BaseStream.Length)
            return;
        byte emitterWho = reader.ReadByte();
        byte kind = reader.ReadByte();
        byte index = reader.ReadByte();
        byte pitchStyleByte = reader.ReadByte();
        bool pitched = reader.ReadBoolean();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        Vector2 pos = new(x, y);

        if (!IsValidPlayer(emitterWho))
            return;
        if (kind > 2)
            return;
        if (index >= 64)
            return;
        if (pitchStyleByte > (byte)BarkPitchStyle.Squeak)
            return;

        SoundPad pad = kind switch
        {
            0 => PuppyPlayer.Barks,
            1 => PuppyPlayer.Cries,
            2 => PuppyPlayer.Growls,
            _ => null
        };
        if (pad == null || pad.Count == 0)
            return;
        index %= (byte)pad.Count;

        bool isSelf = emitterWho == Main.myPlayer;
        var localPP = Main.LocalPlayer.GetModPlayer<PuppyPlayer>();

        // Dedup: if self and we recently predicted a bark, suppress broadcast playback.
        if (isSelf && localPP != null && localPP.ShouldSuppressBroadcastBark())
            return;

        var clientConfig = ModContent.GetInstance<PuppyModClientConfig>();
        SoundStyle baseSound = pad.GetByIndex(index);
        float effVolume = baseSound.Volume * (isSelf ? clientConfig.BarkVolume : clientConfig.OtherBarkVolume);

        // Linear distance attenuation: current volume is close-range, full close is a little louder.
        var serverConfig = ModContent.GetInstance<PuppyModServerConfig>();
        if (serverConfig.BarkRangeTiles > 0)
        {
            float distTiles = Vector2.Distance(pos, Main.LocalPlayer.Center) / 16f;
            float atten = 1f - (distTiles / serverConfig.BarkRangeTiles);
            atten = MathHelper.Clamp(atten, 0f, 1f);
            const float closeBoost = 1.15f;
            atten *= closeBoost;
            effVolume *= atten;
            effVolume = MathHelper.Clamp(effVolume, 0f, 1f);
        }

        // Zero volume = muted, but still do burst for observers if they have ears? Skip sound only.
        if (effVolume > 0.001f)
        {
            float targetPitch = PuppyPlayer.GetPitch((BarkPitchStyle)pitchStyleByte) + (pitched ? 0.3f : 0f);
            targetPitch = MathHelper.Clamp(targetPitch, -1f, 1f);
            SoundStyle toPlay = baseSound with { Pitch = targetPitch, Volume = effVolume, PitchVariance = 0f };
            SoundEngine.PlaySound(toPlay, pos);
        }

        // Observer visual: spawn star burst if emitter has shiny ears. For self we already spawned via predictive path, so only for others or if self not suppressed.
        // Use emitter's PuppyPlayer to decide; if server doesn't sync equipment correctly, fallback to unconditional burst for non-self.
        if (!Main.dedServ)
        {
            Player emitter = Main.player[emitterWho];
            var emitterPP = emitter.GetModPlayer<PuppyPlayer>();
            // If emitter has shiny ears, trigger their OnBark path which spawns burst.
            // We try equipment-aware check; if no ears, we skip burst to preserve original cosmetic condition.
            if (emitterPP != null)
            {
                // Check via public helper; triggers burst only if they have shiny ears.
                emitterPP.TriggerRemoteBarkVisual();
            }
            else if (!isSelf)
            {
                // Fallback: direct burst for observers when PP unavailable - still show something for global bark.
                ShinyEarsItem.SpawnStarBurst(emitter);
            }
        }
    }

    private void HandleServerPet(int patterWho, int targetWho)
    {
        if (!IsValidPlayer(patterWho) || !IsValidPlayer(targetWho))
            return;
        Player patter = Main.player[patterWho];
        PetTarget target = PetTarget.FromPlayer(Main.player[targetWho]);
        if (patter.dead)
            return;
        if (!PetService.CanPet(patter, target))
            return;
        // Server-side throttle: bypassCooldown would otherwise allow spam every tick from a modified
        // client. Clamp to PetRefreshTicks (10) which matches the legitimate hold interval.
        if (Main.GameUpdateCount - PetService.GetTargetLastPetTick(target) < PetService.PetRefreshTicks)
            return;
        // Continuous hold bypasses the 20-tick tap cooldown (PetCooldownTicks). Single-tap spam is
        // still throttled client-side via PatterPetTick; hold refresh sends every PetRefreshTicks (10).
        if (!PetService.ApplyPetCore(patter, target, bypassCooldown: true))
            return;
        BroadcastPet(patterWho, targetWho);
    }

    private void HandleServerPetNpc(int patterWho, int npcWhoAmI)
    {
        if (!IsValidPlayer(patterWho) || !IsValidNpc(npcWhoAmI))
            return;
        Player patter = Main.player[patterWho];
        NPC npc = Main.npc[npcWhoAmI];
        if (patter.dead || npc == null || !npc.active)
            return;
        if (!PetRegistry.IsNpcPettable(npc))
            return;
        PetTarget target = PetTarget.FromNpc(npc);
        if (!PetService.CanPet(patter, target))
            return;
        if (Main.GameUpdateCount - PetService.GetTargetLastPetTick(target) < PetService.PetRefreshTicks)
            return;
        if (!PetService.ApplyPetCore(patter, target, bypassCooldown: true))
            return;
        BroadcastPetNpc(patterWho, npcWhoAmI);
    }

    private static bool IsValidNpc(int npcIndex)
    {
        return npcIndex >= 0 && npcIndex < Main.npc.Length && Main.npc[npcIndex] != null && Main.npc[npcIndex].active;
    }

    /// <summary>Returns whether a packet-supplied player index points at an active player.</summary>
    private static bool IsValidPlayer(int playerIndex)
    {
        return playerIndex >= 0 && playerIndex < Main.player.Length && Main.player[playerIndex] != null && Main.player[playerIndex].active;
    }

    private void HandleServerAttach(int ownerWho, int targetWho, int leashItemType)
    {
        if (!IsValidPlayer(ownerWho) || !IsValidPlayer(targetWho))
            return;
        Player owner = Main.player[ownerWho];
        Player target = Main.player[targetWho];
        if (ownerWho == targetWho)
            return;
        if (!LeashService.CanAttach(owner, target, leashItemType))
            return;
        var chain = target.GetModPlayer<ChainedPlayer>();
        chain.SetGrabberAuthority(ownerWho, leashItemType);
        BroadcastLeashState(ownerWho, targetWho, leashItemType, chain.ActiveCollarItemType);
    }

    private void HandleServerDetach(int ownerWho, int targetWho)
    {
        if (!IsValidPlayer(targetWho))
            return;
        Player target = Main.player[targetWho];
        var chain = target.GetModPlayer<ChainedPlayer>();
        if (chain.GrabberIndex != ownerWho)
            return;
        chain.SetGrabberAuthority(-1, 0);
        BroadcastLeashDetached(targetWho);
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        byte type = reader.ReadByte();
        switch (type)
        {
            case (byte)PuppyPacketType.RequestAttach:
                if (Main.netMode == NetmodeID.Server)
                {
                    if (reader.BaseStream.Position + 1 + 4 > reader.BaseStream.Length) break;
                    HandleServerAttach(whoAmI, reader.ReadByte(), reader.ReadInt32());
                }
                break;
            case (byte)PuppyPacketType.RequestDetach:
                if (Main.netMode == NetmodeID.Server)
                {
                    if (reader.BaseStream.Position + 1 > reader.BaseStream.Length) break;
                    HandleServerDetach(whoAmI, reader.ReadByte());
                }
                break;
            case (byte)PuppyPacketType.Pat:
                if (Main.netMode == NetmodeID.Server)
                {
                    if (reader.BaseStream.Position + 1 > reader.BaseStream.Length) break;
                    HandleServerPet(whoAmI, reader.ReadByte());
                }
                else
                {
                    if (reader.BaseStream.Position + 2 > reader.BaseStream.Length) break;
                    int patterWho = reader.ReadByte();
                    int targetWho = reader.ReadByte();
                    if (IsValidPlayer(targetWho))
                    {
                        Player patter = IsValidPlayer(patterWho) ? Main.player[patterWho] : null;
                        PetTarget target = PetTarget.FromPlayer(Main.player[targetWho]);
                        // Utility applies shared hearts/events; glue adds this mod's sound and reach.
                        PettingPlayer.ApplySyncedPet(patter, target);
                    }
                }
                break;
            case (byte)PuppyPacketType.PetNpc:
                if (Main.netMode == NetmodeID.Server)
                {
                    if (reader.BaseStream.Position + 1 > reader.BaseStream.Length) break;
                    HandleServerPetNpc(whoAmI, reader.ReadByte());
                }
                else
                {
                    if (reader.BaseStream.Position + 2 > reader.BaseStream.Length) break;
                    int patterWho2 = reader.ReadByte();
                    int npcWho = reader.ReadByte();
                    if (IsValidNpc(npcWho))
                    {
                        Player patter2 = IsValidPlayer(patterWho2) ? Main.player[patterWho2] : null;
                        PetTarget target2 = PetTarget.FromNpc(Main.npc[npcWho]);
                        PettingPlayer.ApplySyncedPet(patter2, target2);
                    }
                }
                break;
            case (byte)PuppyPacketType.State:
                if (Main.netMode != NetmodeID.Server)
                {
                    if (reader.BaseStream.Position + 2 + 4 > reader.BaseStream.Length) break;
                    int ownerWho = reader.ReadByte();
                    int targetWho = reader.ReadByte();
                    int leashType = reader.ReadInt32();
                    int collarType = 0;
                    bool hasCollar = false;
                    if (reader.BaseStream.Position + 4 <= reader.BaseStream.Length)
                    {
                        collarType = reader.ReadInt32();
                        hasCollar = true;
                    }
                    if (!IsValidPlayer(targetWho) || (ownerWho != byte.MaxValue && !IsValidPlayer(ownerWho)))
                        break;
                    var chained = Main.player[targetWho].GetModPlayer<ChainedPlayer>();
                    if (hasCollar)
                        chained.ApplyClientState(ownerWho, leashType, collarType);
                    else
                        chained.ApplyClientState(ownerWho, leashType);
                }
                break;
            case (byte)PuppyPacketType.RequestBark:
                if (Main.netMode == NetmodeID.Server)
                    HandleServerBark(whoAmI, reader);
                break;
            case (byte)PuppyPacketType.BarkBroadcast:
                if (Main.netMode != NetmodeID.Server)
                    HandleClientBarkBroadcast(reader);
                break;
        }
    }
}
