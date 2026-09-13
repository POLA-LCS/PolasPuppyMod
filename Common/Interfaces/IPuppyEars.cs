using Terraria;
using PuppyMod.Common.PuppySets;

namespace PuppyMod.Common.Interfaces;

/// <summary>
/// Implemented by anything that counts as Puppy Ears. Stats are full functional-slot values;
/// the equipment entry applies the vanity multiplier centrally.
/// </summary>
public interface IPuppyEars : IPuppyEquipmentProvider
{
    PuppyEarsStats Stats { get; }

    /// <summary>
    /// Purely cosmetic reaction to the set bonus bark.
    /// Called on the local client only, for every equipped ears item (vanity included).
    /// </summary>
    void OnBark(Player player) { }
}
