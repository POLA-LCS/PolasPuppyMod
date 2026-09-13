using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.PuppySets;
using PuppyMod.Players;
using PuppyMod.Services.Leash;

namespace PuppyMod
{
    public enum LeashPacketType : byte
    {
        RequestAttach = 1,
        RequestDetach = 2,
        State = 3
    }

    public class PuppyMod : Mod
    {

        public override uint ExtraPlayerBuffSlots => 1;

        public override void Load()
        {
            PuppyEquipmentRegistry.RegisterDefaults();
            PuppyPairBonusRegistry.RegisterDefaults();
        }

        public override void Unload()
        {
            PuppyEquipmentRegistry.Clear();
            PuppyPairBonusRegistry.Clear();
        }

        public void RequestLeashAttach(int targetWho, int leashItemType)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            var packet = GetPacket();
            packet.Write((byte)LeashPacketType.RequestAttach);
            packet.Write((byte)targetWho);
            packet.Write(leashItemType);
            packet.Send();
        }

        public void RequestLeashDetach(int targetWho)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            var packet = GetPacket();
            packet.Write((byte)LeashPacketType.RequestDetach);
            packet.Write((byte)targetWho);
            packet.Send();
        }

        public void BroadcastLeashState(int ownerWho, int targetWho, int leashItemType, int collarItemType = 0)
        {
            if (Main.netMode != NetmodeID.Server) return;
            var packet = GetPacket();
            packet.Write((byte)LeashPacketType.State);
            packet.Write((byte)ownerWho);
            packet.Write((byte)targetWho);
            packet.Write(leashItemType);
            packet.Write(collarItemType);
            packet.Send();
        }

        public void BroadcastLeashDetached(int targetWho)
        {
            if (Main.netMode != NetmodeID.Server) return;
            var packet = GetPacket();
            packet.Write((byte)LeashPacketType.State);
            packet.Write(byte.MaxValue);
            packet.Write((byte)targetWho);
            packet.Write(0);
            packet.Write(0);
            packet.Send();
        }

        private void HandleServerAttach(int ownerWho, int targetWho, int leashItemType)
        {
            Player owner = Main.player[ownerWho];
            Player target = Main.player[targetWho];
            if (owner == null || target == null) return;
            if (ownerWho == targetWho) return;
            if (!LeashService.CanAttach(owner, target, leashItemType)) return;
            var chain = target.GetModPlayer<ChainedPlayer>();
            chain.SetGrabberAuthority(ownerWho, leashItemType);
            BroadcastLeashState(ownerWho, targetWho, leashItemType, chain.ActiveCollarItemType);
        }

        private void HandleServerDetach(int ownerWho, int targetWho)
        {
            Player target = Main.player[targetWho];
            if (target == null) return;
            var chain = target.GetModPlayer<ChainedPlayer>();
            if (chain.GrabberIndex != ownerWho) return;
            chain.SetGrabberAuthority(-1, 0);
            BroadcastLeashDetached(targetWho);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte type = reader.ReadByte();
            switch (type)
            {
                case (byte)LeashPacketType.RequestAttach:
                    if (Main.netMode == NetmodeID.Server)
                        HandleServerAttach(whoAmI, reader.ReadByte(), reader.ReadInt32());
                    break;
                case (byte)LeashPacketType.RequestDetach:
                    if (Main.netMode == NetmodeID.Server)
                        HandleServerDetach(whoAmI, reader.ReadByte());
                    break;
                case (byte)LeashPacketType.State:
                    if (Main.netMode != NetmodeID.Server)
                    {
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
                        var chained = Main.player[targetWho].GetModPlayer<ChainedPlayer>();
                        if (hasCollar)
                            chained.ApplyClientState(ownerWho, leashType, collarType);
                        else
                            chained.ApplyClientState(ownerWho, leashType);
                    }
                    break;
            }
        }
    }
}
