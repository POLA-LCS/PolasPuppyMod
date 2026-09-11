using System;
using Terraria;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;

namespace PuppyMod.Players;

public class PolasBasePlayer : ModPlayer
{
    // Armor array layout: 0-2 armor, 3-9 accessories, 10-12 vanity armor, 13-19 vanity accessories.
    // Extra accessory slots (Demon Heart / Master) live inside 3-9, so the ranges are fixed.
    private const int HeadSlot = 0;
    private const int AccessorySlotStart = 3;
    private const int AccessorySlotEndExclusive = 10;
    private const int VanityHeadSlot = 10;
    private const int VanityAccessoryStart = 13;

    private static readonly Func<int, bool> IsEarsItem = PuppySetUtils.IsEars;
    private static readonly Func<int, bool> IsTailItem = PuppySetUtils.IsTail;

    public bool HasInAccessory(int itemType) => FindInAccessory(itemType) != 0;
    public bool HasInVanity(int itemType) => FindInVanity(itemType) != 0;

    public int FindInAccessory(int itemType)
    {
        if (itemType == 0) return 0;
        if (IsEarsItem(itemType) && GetSlotType(HeadSlot) == itemType) return itemType;
        return ScanRange(AccessorySlotStart, AccessorySlotEndExclusive, type => type == itemType);
    }

    public int FindInVanity(int itemType)
    {
        if (itemType == 0) return 0;
        if (IsEarsItem(itemType) && GetSlotType(VanityHeadSlot) == itemType) return itemType;
        return ScanRange(VanityAccessoryStart, Player.armor.Length, type => type == itemType);
    }

    public int FindEarsInAccessory() => FindSetItem(HeadSlot, AccessorySlotStart, AccessorySlotEndExclusive, IsEarsItem);
    public int FindEarsInVanity() => FindSetItem(VanityHeadSlot, VanityAccessoryStart, Player.armor.Length, IsEarsItem);
    public int FindTailInAccessory() => ScanRange(AccessorySlotStart, AccessorySlotEndExclusive, IsTailItem);
    public int FindTailInVanity() => ScanRange(VanityAccessoryStart, Player.armor.Length, IsTailItem);

    public void GetDogSetLocationTypes(out int earsAcc, out int earsVan, out int tailAcc, out int tailVan)
    {
        earsAcc = FindEarsInAccessory();
        earsVan = FindEarsInVanity();
        tailAcc = FindTailInAccessory();
        tailVan = FindTailInVanity();
    }

    private int FindSetItem(int specialSlot, int start, int end, Func<int, bool> matches)
    {
        int special = GetSlotType(specialSlot);
        if (special != 0 && matches(special)) return special;
        return ScanRange(start, end, matches);
    }

    private int ScanRange(int start, int end, Func<int, bool> matches)
    {
        end = Math.Min(end, Player.armor.Length);
        for (int i = Math.Max(start, 0); i < end; i++)
        {
            int type = GetSlotType(i);
            if (type != 0 && matches(type)) return type;
        }
        return 0;
    }

    private int GetSlotType(int slot) => slot >= 0 && slot < Player.armor.Length ? Player.armor[slot].type : 0;
}
