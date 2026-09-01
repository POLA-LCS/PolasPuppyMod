using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Data;
using PuppyMod.Common.Interfaces;
using PuppyMod.Services.Leash;

namespace PuppyMod.Players;

public class ChainedPlayer : ModPlayer
{
    public bool hasCollar = false;
    public int? GrabberIndex { get; private set; }
    public int ActiveLeashItemType { get; private set; }
    private int _overstretchTicks;

    private float ActiveLeashRange
    {
        get
        {
            if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is IWithRange leash)
                return leash.RangePixels;
            return 0f;
        }
    }

    private int ActiveLeashRangeTiles
    {
        get
        {
            if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is IWithRange leash)
                return leash.RangeTiles;
            return 0;
        }
    }

    internal void SetGrabberAuthority(int ownerWho, int leashItemType = 0)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        GrabberIndex = ownerWho >= 0 ? ownerWho : null;
        ActiveLeashItemType = GrabberIndex.HasValue ? leashItemType : 0;
        if (Main.netMode == NetmodeID.Server)
        {
            var mod = ModContent.GetInstance<PuppyMod>();
            if (GrabberIndex.HasValue)
                mod.BroadcastLeashState(GrabberIndex.Value, Player.whoAmI, ActiveLeashItemType);
            else
                mod.BroadcastLeashDetached(Player.whoAmI);
        }
    }

    internal void ApplyClientState(int ownerWho, int leashItemType = 0)
    {
        if (Main.netMode == NetmodeID.Server) return;
        GrabberIndex = ownerWho == byte.MaxValue ? (int?)null : ownerWho;
        ActiveLeashItemType = GrabberIndex.HasValue ? leashItemType : 0;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        var packet = Mod.GetPacket();
        packet.Write(PuppyMod.LeashState);
        packet.Write((byte)(GrabberIndex ?? byte.MaxValue));
        packet.Write((byte)Player.whoAmI);
        packet.Write(ActiveLeashItemType);
        if (toWho == -1) packet.Send();
        else packet.Send(toWho);
    }

    private void RestrictMovement(Player owner)
    {
        if (owner == null || !owner.active || owner.dead) return;
        float distance = Vector2.Distance(Player.Center, owner.Center);
        float max = ActiveLeashRange;
        if (distance <= max) return;
        Vector2 midpoint = (Player.Center + owner.Center) / 2f;
        Vector2 puppyOffset = Player.Center - midpoint;
        Vector2 ownerOffset = owner.Center - midpoint;
        float puppyPull = 0.10f;
        float ownerPull = 0.018f;
        if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leash)
        {
            puppyPull = leash.PuppyPull;
            ownerPull = leash.OwnerPull;
        }
        const float div = 8f;
        Player.velocity -= puppyOffset * puppyPull / div;
        owner.velocity -= ownerOffset * ownerPull / div;
    }

    private void ApplyLeashPhysics(Player owner)
    {
        if (owner == null || !owner.active || owner.dead) return;
        int rangeTiles = ActiveLeashRangeTiles;
        if (rangeTiles <= 0)
        {
            RestrictMovement(owner);
            return;
        }
        if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leash)
        {
            int tiles = leash.RangeTiles;
            if (tiles <= 0)
                tiles = rangeTiles;
            LeashPhysicsProfile profile = leash.Physics;
            var result = LeashPhysicsService.Compute(Player.Center, owner.Center, Player.velocity, owner.velocity, tiles, profile);
            if (result.Overstretched)
                _overstretchTicks++;
            else if (_overstretchTicks > 0)
                _overstretchTicks--;

            if (result.IsTaut)
            {
                Player.velocity += result.PuppyImpulse;
                owner.velocity += result.OwnerImpulse;
            }
        }
        else
        {
            RestrictMovement(owner);
        }
    }

    private bool IsChainValid()
    {
        if (!Player.GetModPlayer<PuppyPlayer>().IsPuppy) return false;
        if (!hasCollar) return false;
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
        hasCollar = false;
    }

    public override void PostUpdateEquips()
    {
        if (hasCollar)
        {
            Player.statDefense += 2;
            Lighting.AddLight(Player.Center, 0.4f, 0.3f, 0.15f);
        }
    }

    public override void PostUpdate()
    {
        if (!GrabberIndex.HasValue) return;
        if (!IsChainValid())
        {
            if (Main.netMode == NetmodeID.Server)
                ModContent.GetInstance<PuppyMod>().BroadcastLeashDetached(Player.whoAmI);
            GrabberIndex = null;
            ActiveLeashItemType = 0;
            return;
        }
        if (ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leash)
            leash.AffectPuppy(Player);
        Player.AddBuff(BuffID.Sunflower, 60);
        ApplyLeashPhysics(OwnerOf);
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (GrabberIndex.HasValue && Main.netMode == NetmodeID.Server)
            ModContent.GetInstance<PuppyMod>().BroadcastLeashDetached(Player.whoAmI);
        GrabberIndex = null;
        ActiveLeashItemType = 0;
    }

    private void DrawRope(Player owner, PlayerDrawSet drawInfo)
    {
        _ = drawInfo;
        Vector2 start = Player.Center;
        Vector2 end = owner.Center;
        Vector2 direction = end - start;
        float length = direction.Length();
        direction.Normalize();
        string texPath = "Terraria/Images/Chain";
        if (ActiveLeashItemType != 0 && ModContent.GetModItem(ActiveLeashItemType) is ILeashItem leash)
            texPath = leash.LeashTexturePath;
        Texture2D ropeTexture = ModContent.Request<Texture2D>(texPath).Value;
        Color ropeColor = Color.White;
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
