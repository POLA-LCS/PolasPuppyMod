using Terraria;
using Terraria.ModLoader;

namespace PuppyMod;

public class OwnerPlayer : ModPlayer
{
    // Future Range for UI
    // public float ClickRangeHeld { get; private set; }

    public float ClickRange { get; private set; }
    public int BuffDuration { get; private set; }

    // Window for preventing puppies entering the range of the clicker in cooldown
    public int ClickSignalTimer { get; private set; }
    public int ClickCooldown { get; private set; }

    public bool HasClicked => ClickSignalTimer > 0;
    public bool CanClick => ClickCooldown <= 0;

    public void TriggerClick(float rangeInPixels, int buffDurationTicks, int cooldownTicks)
    {
        ClickRange = rangeInPixels;
        BuffDuration = buffDurationTicks;
        // The idea is to affect only at the instant of click
        ClickSignalTimer = 10;
        ClickCooldown = cooldownTicks;

        // ClickRangeHeld = rangeInPixels;
    }

    // public override void PreUpdate()
    // {
    //     if (Player.HeldItem.ModItem is BaseClickerItem clicker)
    //     {
    //         ClickRangeHeld = clicker.TileRange * 16f;
    //     }
    //     else
    //     {
    //         ClickRangeHeld = 0f;
    //     }
    // }

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
