using Terraria;
using Terraria.ModLoader;

namespace PuppyMod.Players;

public class PolasBasePlayer : ModPlayer
{
    /// <summary>
    /// This is not intended to be used with modded item
    /// </summary>
    /// <param name="item">Vanilla ItemID</param>
    /// <param name="accessory">Check for accessory</param>
    /// <param name="vanity">Check for vanity</param>
    /// <returns>true accessory, false vanity, null neither</returns>
    public bool? HasEquippedAccessoryVanity(short item, bool accessory = true, bool vanity = true)
    {
        int extra = Player.GetAmountOfExtraAccessorySlotsToShow();

        if (accessory)
        {
            for (int i = 3; i < 10 + extra && i < Player.armor.Length; i++)
            {
                if (Player.armor[i].type == item)
                    return true;
            }
        }
        if (vanity)
        {
            for (int i = 13; i < Player.armor.Length; i++)
            {
                if (Player.armor[i].type == item)
                    return false;
            }
        }

        return null;
    }
}