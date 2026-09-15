using PuppyMod.Common.PuppySets.Core;

namespace PuppyMod.Common.PuppySets.Definitions;

public abstract class PuppyEquipmentDefinition(
    int itemType,
    PuppyEquipmentKind kind,
    PuppyFamily family,
    IPuppyEquipmentProvider provider,
    PuppyTooltipDefinition tooltip)
{

    public int ItemType { get; } = itemType;
    public PuppyEquipmentKind Kind { get; } = kind;
    public PuppyFamily Family { get; } = family;
    public IPuppyEquipmentProvider Provider { get; } = provider;
    public PuppyTooltipDefinition Tooltip { get; } = tooltip;
}
