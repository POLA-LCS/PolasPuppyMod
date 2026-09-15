namespace PuppyMod.Common.PuppySets.Core;

/// <summary>
/// Animated vanity sheets and their registered equip texture names.
/// Each sheet is a 40x1120 equip sheet (20 frames of 40x56), animated by the player's body frame.
/// Replace/recolor the referenced PNG files to re-skin a set; no code changes needed.
/// </summary>
public static class PuppyEquipmentTextures
{
    public const string ShinyEarsSheet = "PuppyMod/Assets/Armor/ShinySet_Ears";
    public const string ReinforcedEarsSheet = "PuppyMod/Assets/Armor/ReinforcedSet_Ears";
    public const string ShinyTailSheet = "PuppyMod/Assets/Armor/ShinySet_Tail";
    public const string ReinforcedTailSheet = "PuppyMod/Assets/Armor/ReinforcedSet_Tail";

    public const string ShinySetEarsHead = "ShinySetEarsHead";
    public const string ReinforcedSetEarsHead = "ReinforcedSetEarsHead";
    public const string ShinySetTailBack = "ShinySetTailBack";
    public const string ReinforcedSetTailBack = "ReinforcedSetTailBack";
}
