using PuppyMod.Common.PuppySets.Definitions;

namespace PuppyMod.Common.PuppySets.Core;

/// <summary>
/// Placement state of the selected Puppy set pair.
/// Costume = all pieces vanity, Furry = exactly one piece functional, Therian = all pieces functional.
/// </summary>
public enum PuppySetPlacement
{
    Costume,
    Furry,
    Therian
}

/// <summary>
/// Helper for deriving placement from equipped entries. Lives in Core so both State and Bonuses can call it without a cycle.
/// </summary>
public static class PuppySetPlacementHelper
{
    public static PuppySetPlacement FromEntries(PuppyEquipmentEntry? ears, PuppyEquipmentEntry? tail)
    {
        int functional = (ears?.IsFunctional == true ? 1 : 0) + (tail?.IsFunctional == true ? 1 : 0);
        return functional switch
        {
            2 => PuppySetPlacement.Therian,
            1 => PuppySetPlacement.Furry,
            _ => PuppySetPlacement.Costume
        };
    }

    public static PuppySetPlacement FromFunctional(bool earsFunctional, bool tailFunctional)
    {
        int functional = (earsFunctional ? 1 : 0) + (tailFunctional ? 1 : 0);
        return functional switch
        {
            2 => PuppySetPlacement.Therian,
            1 => PuppySetPlacement.Furry,
            _ => PuppySetPlacement.Costume
        };
    }
}
