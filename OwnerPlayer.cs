using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod;

public class OwnerPlayer : ModPlayer
{
    public float ClickRange { get; private set; }
    public int BuffDuration { get; private set; }

    public int ClickSignalTimer { get; private set; }
    public int ClickCooldown { get; private set; }

    private bool prevMouseRight = false;

    public bool HasClicked => ClickSignalTimer > 0;
    public bool CanClick => ClickCooldown <= 0;

    public void TriggerClick(float rangeInPixels, int buffDurationTicks, int cooldownTicks)
    {
        ClickRange = rangeInPixels;
        BuffDuration = buffDurationTicks;
        ClickSignalTimer = 10;
        ClickCooldown = cooldownTicks;
    }

    private void HandleLeashInput()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        // Only an owner (a human) can leash - never a puppy.
        if (Player.GetModPlayer<PuppyPlayer>().IsPuppy)
            return;

        bool curMouseRight = Main.mouseRight;
        bool freshlyPressed = curMouseRight && !prevMouseRight;
        prevMouseRight = curMouseRight;

        if (!freshlyPressed)
            return;

        foreach (Player target in Main.player)
        {
            if (target == null || !target.active || target.dead) continue;
            if (target.whoAmI == Player.whoAmI) continue;

            // Only actual puppies can be leashed.
            if (!target.GetModPlayer<PuppyPlayer>().IsPuppy) continue;

            // The puppy must be wearing a Leash Collar.
            var chain = target.GetModPlayer<ChainedPlayer>();
            if (!chain.hasChainLeash) continue;

            // Cursor must be over the puppy to toggle.
            if (!target.Hitbox.Contains(Main.MouseWorld.ToPoint())) continue;

            bool alreadyGrabbedByMe = chain.GrabberIndex == Player.whoAmI;

            if (alreadyGrabbedByMe)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    ModContent.GetInstance<PuppyMod>().RequestLeashDetach(target.whoAmI);
                else
                    chain.SetGrabberAuthority(-1);
            }
            else
            {
                if (Vector2.Distance(Player.Center, target.Center) > ChainedPlayer.MaxDistance) continue;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    ModContent.GetInstance<PuppyMod>().RequestLeashAttach(target.whoAmI);
                else
                    chain.SetGrabberAuthority(Player.whoAmI);
            }
        }
    }

    public override void PreUpdate()
    {
        HandleLeashInput();
    }

    public override void PostUpdate()
    {
        if (ClickSignalTimer > 0)
            ClickSignalTimer--;

        if (ClickCooldown > 0)
            ClickCooldown--;
    }

    public override void Kill(double damage, int hitDirection, bool pvp, Terraria.DataStructures.PlayerDeathReason damageSource)
    {
        ClickSignalTimer = 0;
        ClickCooldown = 0;
        ClickRange = 0f;
    }
}
