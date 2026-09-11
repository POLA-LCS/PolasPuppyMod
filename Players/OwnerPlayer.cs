using Terraria;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;

namespace PuppyMod.Players;

public class OwnerPlayer : ModPlayer
{
    public const int ClickSignalTicks = 10;
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
        ClickSignalTimer = ClickSignalTicks;
        ClickCooldown = cooldownTicks;
    }

    public override void PreUpdate()
    {
    }

    public override void PostUpdateEquips()
    {
        for (int i = 0; i < Main.player.Length; i++)
        {
            Player puppy = Main.player[i];
            if (puppy == null || !puppy.active || puppy.dead)
                continue;

            var chained = puppy.GetModPlayer<ChainedPlayer>();
            if (chained.GrabberIndex != Player.whoAmI)
                continue;

            if (chained.ActiveCollarItemType != 0 && ModContent.GetModItem(chained.ActiveCollarItemType) is ICollarItem collar)
                collar.AffectOwner(Player);
        }
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
