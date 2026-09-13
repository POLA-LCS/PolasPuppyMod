using System;
using System.Linq;

namespace PuppyMod.Common.PuppySets;

public static class PuppyEquipmentResolver
{
    public static PuppyEquipmentResolution Resolve(PuppyEquipmentSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        PuppyEquipmentEntry selectedEars = snapshot.Ears
            .OrderBy(GetEarsPriority)
            .ThenBy(entry => entry.Slot)
            .FirstOrDefault();

        PuppyEquipmentEntry[] validTails = snapshot.Tails
            .Where(entry => entry.IsAccessorySlot)
            .OrderBy(GetTailPriority)
            .ThenBy(entry => entry.Slot)
            .ToArray();

        PuppyEquipmentEntry primaryTail = validTails.FirstOrDefault();
        PuppyEquipmentEntry selectedTail = null;
        if (selectedEars != null && selectedEars.Family != PuppyFamily.None)
        {
            // Filter by family before applying the functional/vanity priority. This keeps
            // a nonmatching functional Tail from blocking a matching vanity Tail.
            selectedTail = validTails
                .Where(entry => entry.Family == selectedEars.Family)
                .OrderBy(GetTailPriority)
                .ThenBy(entry => entry.Slot)
                .FirstOrDefault();
        }

        PuppyEquipmentEntry effectiveTail = selectedTail ?? primaryTail;
        PuppyEquipmentStats stats = BuildStats(selectedEars, effectiveTail);

        return new PuppyEquipmentResolution(
            snapshot,
            Array.AsReadOnly(validTails),
            selectedEars,
            primaryTail,
            selectedTail,
            stats);
    }

    private static int GetEarsPriority(PuppyEquipmentEntry entry)
    {
        return entry.Location switch
        {
            PuppyEquipmentSlotLocation.FunctionalHead => 0,
            PuppyEquipmentSlotLocation.FunctionalAccessory => 1,
            PuppyEquipmentSlotLocation.VanityHead => 2,
            PuppyEquipmentSlotLocation.VanityAccessory => 3,
            _ => 4
        };
    }

    private static int GetTailPriority(PuppyEquipmentEntry entry)
    {
        return entry.Location switch
        {
            PuppyEquipmentSlotLocation.FunctionalAccessory => 0,
            PuppyEquipmentSlotLocation.VanityAccessory => 1,
            _ => 2
        };
    }

    private static PuppyEquipmentStats BuildStats(PuppyEquipmentEntry ears, PuppyEquipmentEntry tail)
    {
        PuppyEarsStats earsStats = default;
        if (ears != null)
            earsStats = ears.EarsProvider.Stats.Scale(ears.ValueMultiplier);

        PuppyTailStats tailStats = default;
        if (tail != null)
            tailStats = tail.TailProvider.Stats.Scale(tail.ValueMultiplier);

        return new PuppyEquipmentStats(
            earsStats.PickSpeed,
            tailStats.MoveSpeed,
            tailStats.AccRunSpeed,
            tailStats.MaxRunSpeed,
            tailStats.JumpSpeedBoost);
    }
}
