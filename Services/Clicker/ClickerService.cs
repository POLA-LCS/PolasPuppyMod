using Terraria;
using PuppyMod.Players;

namespace PuppyMod.Services.Clicker;

public static class ClickerService
{
    public static void Trigger(Player owner, float rangePx, int buffTicks, int cooldown)
    {
        var op = owner.GetModPlayer<OwnerPlayer>();
        op.TriggerClick(rangePx, buffTicks, cooldown);
    }
}
