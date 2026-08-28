using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod
{
    public class PuppyMod : Mod
    {
        public const byte LeashReqAttach = 1;
        public const byte LeashReqDetach = 2;
        public const byte LeashState = 3;

        public override uint ExtraPlayerBuffSlots => 1;

        public void RequestLeashAttach(int targetWho)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            var packet = GetPacket();
            packet.Write(LeashReqAttach);
            packet.Write((byte)targetWho);
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

        public void BroadcastLeashState(int ownerWho, int targetWho)
        {
            if (Main.netMode != NetmodeID.Server) return;
            var packet = GetPacket();
            packet.Write(LeashState);
            packet.Write((byte)ownerWho);
            packet.Write((byte)targetWho);
            packet.Send();
        }

        public void BroadcastLeashDetached(int targetWho)
        {
            if (Main.netMode != NetmodeID.Server) return;
            var packet = GetPacket();
            packet.Write(LeashState);
            packet.Write(byte.MaxValue);
            packet.Write((byte)targetWho);
            packet.Send();
        }

        private void HandleServerAttach(int ownerWho, int targetWho)
        {
            Player owner = Main.player[ownerWho];
            Player target = Main.player[targetWho];
            if (owner == null || target == null) return;
            if (ownerWho == targetWho) return;
            if (owner.GetModPlayer<PuppyPlayer>().IsPuppy) return;
            if (!target.GetModPlayer<PuppyPlayer>().IsPuppy) return;
            if (!target.GetModPlayer<ChainedPlayer>().hasChainLeash) return;
            if (Vector2.Distance(owner.Center, target.Center) > ChainedPlayer.MaxDistance) return;
            target.GetModPlayer<ChainedPlayer>().SetGrabberAuthority(ownerWho);
            BroadcastLeashState(ownerWho, targetWho);
        }

        private void HandleServerDetach(int ownerWho, int targetWho)
        {
            Player target = Main.player[targetWho];
            if (target == null) return;
            var chain = target.GetModPlayer<ChainedPlayer>();
            if (chain.GrabberIndex != ownerWho) return;
            chain.SetGrabberAuthority(-1);
            BroadcastLeashDetached(targetWho);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte type = reader.ReadByte();
            switch (type)
            {
                case LeashReqAttach:
                    if (Main.netMode == NetmodeID.Server)
                        HandleServerAttach(whoAmI, reader.ReadByte());
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
                        Main.player[targetWho].GetModPlayer<ChainedPlayer>().ApplyClientState(ownerWho);
                    }
                    break;
            }
        }
    }
}
