using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets.Core;

namespace PuppyMod.Common.PuppySets.Definitions;

public sealed class PuppyEquipmentEntry(PuppyEquipmentDefinition definition, int slot, PuppyEquipmentSlotLocation location)
{

    public PuppyEquipmentDefinition Definition { get; } = definition;
    public int ItemType => Definition.ItemType;
    public PuppyEquipmentKind Kind => Definition.Kind;
    public PuppyFamily Family => Definition.Family;
    public int Slot { get; } = slot;
    public PuppyEquipmentSlotLocation Location { get; } = location;
    public bool IsFunctional => Location == PuppyEquipmentSlotLocation.FunctionalHead || Location == PuppyEquipmentSlotLocation.FunctionalAccessory;
    public bool IsAccessorySlot => Location == PuppyEquipmentSlotLocation.FunctionalAccessory || Location == PuppyEquipmentSlotLocation.VanityAccessory;
    /// <summary>Functional entries count at full value, vanity entries at half.</summary>
    public float ValueMultiplier => IsFunctional ? 1f : 0.5f;

    public IPuppyEquipmentProvider Provider => Definition.Provider;
    public IPuppyEarsItem EarsProvider => Definition.Provider as IPuppyEarsItem;
    public IPuppyTailItem TailProvider => Definition.Provider as IPuppyTailItem;
}
