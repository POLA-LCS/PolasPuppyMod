using Terraria;

namespace PuppyMod.Common.Interfaces;

/// <summary>
/// Implemented by anything that counts as "dog ears" for the puppy set.
/// Each ears item owns its own stats - the puppy set only cares about which item is equipped
/// and where it is sitting (functional slot / vanity slot).
/// </summary>
public interface IPuppyEars
{
    /// <summary>pickSpeed reduction granted while worn in a functional slot.</summary>
    float PickSpeedAccessory { get; }

    /// <summary>pickSpeed reduction granted while worn in a vanity slot.</summary>
    float PickSpeedVanity { get; }

    /// <summary>
    /// Purely cosmetic reaction to the set bonus bark.
    /// Called on the local client only, for every equipped ears item (vanity included).
    /// </summary>
    void OnBark(Player player) { }
}
