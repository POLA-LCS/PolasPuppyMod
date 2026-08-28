using Terraria;
using Terraria.ModLoader;

namespace PuppyMod;

public class OwnerPlayer : ModPlayer
{
    public float ClickRange { get; private set; }
    public int BuffDuration { get; private set; }

    public int ClickSignalTimer { get; private set; }
    public int ClickCooldown { get; private set; }

    public bool HasClicked => ClickSignalTimer > 0;
    public bool CanClick => ClickCooldown <= 0;

    public void TriggerClick(float rangeInPixels, int buffDurationTicks, int cooldownTicks)
    {
        ClickRange = rangeInPixels;
        BuffDuration = buffDurationTicks;
        ClickSignalTimer = 10;
        ClickCooldown = cooldownTicks;
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
