using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod;

public class ChainedPlayer : ModPlayer
{
    public const float MaxDistance = 15f * 16f;
    private const string RopeTexturePath = "Terraria/Images/Chain";

    public bool hasChainLeash = false;
    public int? GrabberIndex { get; private set; }

    internal void SetGrabberAuthority(int ownerWho)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        GrabberIndex = ownerWho >= 0 ? ownerWho : null;
        if (Main.netMode == NetmodeID.Server)
        {
            var mod = ModContent.GetInstance<PuppyMod>();
            if (GrabberIndex.HasValue)
                mod.BroadcastLeashState(GrabberIndex.Value, Player.whoAmI);
            else
                mod.BroadcastLeashDetached(Player.whoAmI);
        }
    }

    internal void ApplyClientState(int ownerWho)
    {
        if (Main.netMode == NetmodeID.Server) return;
        GrabberIndex = ownerWho == byte.MaxValue ? (int?)null : ownerWho;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        var packet = Mod.GetPacket();
        packet.Write(PuppyMod.LeashState);
        packet.Write((byte)(GrabberIndex ?? byte.MaxValue));
        packet.Write((byte)Player.whoAmI);
        if (toWho == -1) packet.Send();
        else packet.Send(toWho);
    }

    private void RestrictMovement(Player owner)
    {
        if (owner == null || !owner.active || owner.dead) return;
        float distance = Vector2.Distance(Player.Center, owner.Center);
        if (distance <= MaxDistance) return;
        Vector2 midpoint = (Player.Center + owner.Center) / 2f;
        Vector2 puppyOffset = Player.Center - midpoint;
        Vector2 ownerOffset = owner.Center - midpoint;
        const float puppyPull = 0.10f;
        const float ownerPull = 0.018f;
        Player.velocity -= puppyOffset * puppyPull / 8f;
        owner.velocity -= ownerOffset * ownerPull / 8f;
    }

    private bool IsChainValid()
    {
        if (!Player.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
        if (!hasChainLeash) return false;
        Player owner = OwnerOf;
        if (owner == null || !owner.active || owner.dead) return false;
        if (owner.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
        return true;
    }

    private Player OwnerOf
    {
        get
        {
            if (GrabberIndex is int i && i >= 0 && i < Main.player.Length)
                return Main.player[i];
            return null;
        }
    }

    public override void ResetEffects()
    {
        hasChainLeash = false;
    }

    public override void PostUpdate()
    {
        if (!GrabberIndex.HasValue) return;
        if (!IsChainValid())
        {
            if (Main.netMode == NetmodeID.Server)
                ModContent.GetInstance<PuppyMod>().BroadcastLeashDetached(Player.whoAmI);
            GrabberIndex = null;
            return;
        }
        Player.AddBuff(BuffID.Sunflower, 60);
        RestrictMovement(OwnerOf);
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (GrabberIndex.HasValue && Main.netMode == NetmodeID.Server)
            ModContent.GetInstance<PuppyMod>().BroadcastLeashDetached(Player.whoAmI);
        GrabberIndex = null;
    }

    private void DrawRope(Player owner, PlayerDrawSet drawInfo)
    {
        _ = drawInfo;
        Vector2 start = Player.Center;
        Vector2 end = owner.Center;
        Vector2 direction = end - start;
        float length = direction.Length();
        direction.Normalize();
        Texture2D ropeTexture = ModContent.Request<Texture2D>(RopeTexturePath).Value;
        Color ropeColor = new Color(193, 154, 107);
        for (float i = 0; i < length; i += ropeTexture.Width)
        {
            Vector2 position = start + direction * i - Main.screenPosition;
            Main.EntitySpriteDraw(
                ropeTexture,
                position,
                null,
                ropeColor,
                direction.ToRotation() + MathHelper.PiOver2,
                new Vector2(ropeTexture.Width / 2f, ropeTexture.Height / 2f),
                1f,
                SpriteEffects.None,
                0
            );
        }
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (!GrabberIndex.HasValue) return;
        Player owner = OwnerOf;
        if (owner == null || !owner.active || owner.dead) return;
        DrawRope(owner, drawInfo);
    }
}
