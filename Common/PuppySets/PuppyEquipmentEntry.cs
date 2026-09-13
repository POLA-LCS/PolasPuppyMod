using PuppyMod.Common.Interfaces;

namespace PuppyMod.Common.PuppySets;

public sealed class PuppyEquipmentEntry
{
    public PuppyEquipmentEntry(PuppyEquipmentDefinition definition, int slot, PuppyEquipmentSlotLocation location)
    {
        Definition = definition;
        Slot = slot;
        Location = location;
    }

    public PuppyEquipmentDefinition Definition { get; }
    public int ItemType => Definition.ItemType;
    public PuppyEquipmentKind Kind => Definition.Kind;
    public PuppyFamily Family => Definition.Family;
    public int Slot { get; }
    public PuppyEquipmentSlotLocation Location { get; }
    public bool IsFunctional => Location == PuppyEquipmentSlotLocation.FunctionalHead || Location == PuppyEquipmentSlotLocation.FunctionalAccessory;
    public bool IsAccessorySlot => Location == PuppyEquipmentSlotLocation.FunctionalAccessory || Location == PuppyEquipmentSlotLocation.VanityAccessory;
    /// <summary>Uniform halving: functional 1f, vanity 0.5f centrally for all PuppyEquipmentStats; collar/leash functional-only (no vanity).</summary>
    public float ValueMultiplier => IsFunctional ? 1f : 0.5f;

    public IPuppyEquipmentProvider Provider => Definition.Provider;
    public IPuppyEarsItem EarsProvider => Definition.Provider as IPuppyEarsItem;
    public IPuppyTailItem TailProvider => Definition.Provider as IPuppyTailItem;
}
