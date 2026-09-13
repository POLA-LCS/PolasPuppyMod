using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace PuppyMod.Common.PuppySets;

/// <summary>
/// Scans the vanilla armor array layout. No custom accessory slots are used by this mod.
/// </summary>
public static class PuppyEquipmentScanner
{
    private const int FunctionalHeadSlot = 0;
    private const int FunctionalAccessoryStart = 3;
    private const int FunctionalAccessoryEndExclusive = 10;
    private const int VanityHeadSlot = 10;
    private const int VanityAccessoryStart = 13;
    private const int VanityAccessoryEndExclusive = 20;

    public static PuppyEquipmentSnapshot Scan(Player player)
    {
        var entries = new List<PuppyEquipmentEntry>();

        AddSlot(player, entries, FunctionalHeadSlot, PuppyEquipmentSlotLocation.FunctionalHead);
        for (int slot = FunctionalAccessoryStart; slot < FunctionalAccessoryEndExclusive; slot++)
            AddSlot(player, entries, slot, PuppyEquipmentSlotLocation.FunctionalAccessory);

        AddSlot(player, entries, VanityHeadSlot, PuppyEquipmentSlotLocation.VanityHead);
        for (int slot = VanityAccessoryStart; slot < VanityAccessoryEndExclusive; slot++)
            AddSlot(player, entries, slot, PuppyEquipmentSlotLocation.VanityAccessory);

        return new PuppyEquipmentSnapshot(entries);
    }

    private static void AddSlot(
        Player player,
        List<PuppyEquipmentEntry> entries,
        int slot,
        PuppyEquipmentSlotLocation location)
    {
        if (slot < 0 || slot >= player.armor.Length)
            return;

        Item item = player.armor[slot];
        if (item == null || item.type == ItemID.None)
            return;

        if (!PuppyEquipmentRegistry.TryGetDefinition(item.type, out PuppyEquipmentDefinition definition))
            return;

        entries.Add(new PuppyEquipmentEntry(definition, slot, location));
    }
}
