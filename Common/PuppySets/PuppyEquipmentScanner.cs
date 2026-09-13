using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace PuppyMod.Common.PuppySets;

/// <summary>
/// Scans the vanilla armor array layout plus dynamic extra accessory slots (Demon Heart / Master).
/// H6: Previously hardcoded FunctionalAccessory 3-9 only (vanilla 20 slots); now iterates beyond 20 for dynamic slots.
/// </summary>
public static class PuppyEquipmentScanner
{
    private const int FunctionalHeadSlot = 0;
    private const int FunctionalAccessoryStart = 3;
    private const int FunctionalAccessoryEndExclusive = 10;
    private const int VanityHeadSlot = 10;
    private const int VanityAccessoryStart = 13;
    private const int VanityAccessoryEndExclusive = 20;
    // H6: Dynamic extra slots start at 20 (vanilla length) – Demon Heart / Master Mode adds functional accessory slots beyond.

    public static PuppyEquipmentSnapshot Scan(Player player)
    {
        var entries = new List<PuppyEquipmentEntry>();

        AddSlot(player, entries, FunctionalHeadSlot, PuppyEquipmentSlotLocation.FunctionalHead);
        for (int slot = FunctionalAccessoryStart; slot < FunctionalAccessoryEndExclusive; slot++)
            AddSlot(player, entries, slot, PuppyEquipmentSlotLocation.FunctionalAccessory);

        AddSlot(player, entries, VanityHeadSlot, PuppyEquipmentSlotLocation.VanityHead);
        for (int slot = VanityAccessoryStart; slot < VanityAccessoryEndExclusive; slot++)
            AddSlot(player, entries, slot, PuppyEquipmentSlotLocation.VanityAccessory);

        // H6: Iterate dynamic extra accessory slots beyond vanilla 20 for Demon Heart/Master; treat as FunctionalAccessory if TryGetDefinition succeeds.
        // Also supports mods that extend armor.Length – loop 20..Length with functional mapping.
        for (int slot = 20; slot < player.armor.Length; slot++)
            AddSlot(player, entries, slot, PuppyEquipmentSlotLocation.FunctionalAccessory);

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
