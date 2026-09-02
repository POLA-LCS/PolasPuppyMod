using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Players;

public class PolasBasePlayer : ModPlayer
{
    public bool HasInAccessory(short itemType)
    {
        if (itemType == ItemID.DogEars)
        {
            if (Player.armor[0].type == itemType) return true;
        }
        int extra = Player.GetAmountOfExtraAccessorySlotsToShow();
        for (int i = 3; i < 10 + extra && i < Player.armor.Length; i++)
            if (Player.armor[i].type == itemType) return true;
        return false;
    }
    public bool HasInVanity(short itemType)
    {
        if (itemType == ItemID.DogEars)
        {
            if (Player.armor.Length > 10 && Player.armor[10].type == itemType) return true;
        }
        for (int i = 13; i < Player.armor.Length; i++)
            if (Player.armor[i].type == itemType) return true;
        return false;
    }
    public void GetDogSetLocationFlags(out bool earsAcc, out bool earsVan, out bool tailAcc, out bool tailVan)
    {
        earsAcc = HasInAccessory(ItemID.DogEars);
        earsVan = HasInVanity(ItemID.DogEars);
        tailAcc = HasInAccessory(ItemID.DogTail);
        tailVan = HasInVanity(ItemID.DogTail);
    }
    [Obsolete("Use HasInAccessory/HasInVanity; early-return misses duplicates.")]
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