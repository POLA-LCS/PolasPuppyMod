namespace PuppyMod.Common.PuppySets;

/// <summary>
/// Animated vanity sheets and their registered equip texture names.
/// Each sheet is a 40x1120 equip sheet (20 frames of 40x56), animated by the player's body frame.
/// Replace/recolor the referenced PNG files to re-skin a set; no code changes needed.
/// </summary>
public static class PuppyEquipmentTextures
{
    public const string ShinyEarsSheet = "PuppyMod/Assets/Armor/ShinyEarsArmorSpreadsheet";
    public const string ReinforcedEarsSheet = "PuppyMod/Assets/Armor/ReinforcedEarsArmorSpreadsheet";
    public const string ShinyTailSheet = "PuppyMod/Assets/Armor/ShinyTailArmorSpreadsheet";
    public const string ReinforcedTailSheet = "PuppyMod/Assets/Armor/ReinforcedTailArmorSpreadsheet";

    public const string ShinyEarsHead = "ShinyEarsHead";
    public const string ReinforcedEarsHead = "ReinforcedEarsHead";
    public const string ShinyTailBack = "ShinyTailBack";
    public const string ReinforcedTailBack = "ReinforcedTailBack";
}
