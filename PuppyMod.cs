using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Players;
using PuppyMod.Services.Leash;

namespace PuppyMod
{
    public class PuppyMod : Mod
    {
        public const byte LeashReqAttach = 1;
        public const byte LeashReqDetach = 2;
        public const byte LeashState = 3;

        public override uint ExtraPlayerBuffSlots => 1;

        public void RequestLeashAttach(int targetWho, int leashItemType)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            var packet = GetPacket();
            packet.Write(LeashReqAttach);
            packet.Write((byte)targetWho);
            packet.Write(leashItemType);
            packet.Send();
        }

        public void RequestLeashDetach(int targetWho)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            var packet = GetPacket();
            packet.Write(LeashReqDetach);
            packet.Write((byte)targetWho);
            packet.Send();
        }

        public void BroadcastLeashState(int ownerWho, int targetWho, int leashItemType)
        {
            if (Main.netMode != NetmodeID.Server) return;
            var packet = GetPacket();
            packet.Write(LeashState);
            packet.Write((byte)ownerWho);
            packet.Write((byte)targetWho);
            packet.Write(leashItemType);
            packet.Send();
        }

        public void BroadcastLeashDetached(int targetWho)
        {
            if (Main.netMode != NetmodeID.Server) return;
            var packet = GetPacket();
            packet.Write(LeashState);
            packet.Write(byte.MaxValue);
            packet.Write((byte)targetWho);
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
            BroadcastLeashState(ownerWho, targetWho, leashItemType);
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
                case LeashReqAttach:
                    if (Main.netMode == NetmodeID.Server)
                        HandleServerAttach(whoAmI, reader.ReadByte(), reader.ReadInt32());
                    break;
                case LeashReqDetach:
                    if (Main.netMode == NetmodeID.Server)
                        HandleServerDetach(whoAmI, reader.ReadByte());
                    break;
                case LeashState:
                    if (Main.netMode != NetmodeID.Server)
                    {
                        int ownerWho = reader.ReadByte();
                        int targetWho = reader.ReadByte();
                        int leashType = reader.ReadInt32();
                        Main.player[targetWho].GetModPlayer<ChainedPlayer>().ApplyClientState(ownerWho, leashType);
                    }
                    break;
            }
        }
    }
}
