namespace PuppyMod.Common.PuppySets;

public abstract class PuppyEquipmentDefinition
{
    protected PuppyEquipmentDefinition(
        int itemType,
        PuppyEquipmentKind kind,
        PuppyFamily family,
        IPuppyEquipmentProvider provider,
        PuppyTooltipDefinition tooltip)
    {
        ItemType = itemType;
        Kind = kind;
        Family = family;
        Provider = provider;
        Tooltip = tooltip;
    }

    public int ItemType { get; }
    public PuppyEquipmentKind Kind { get; }
    public PuppyFamily Family { get; }
    public IPuppyEquipmentProvider Provider { get; }
    public PuppyTooltipDefinition Tooltip { get; }
}
