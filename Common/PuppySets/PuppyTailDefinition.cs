using PuppyMod.Common.Interfaces;

namespace PuppyMod.Common.PuppySets;

public sealed class PuppyTailDefinition(
    int itemType,
    PuppyFamily family,
    IPuppyTailItem provider,
    PuppyTooltipDefinition tooltip) : PuppyEquipmentDefinition(itemType, PuppyEquipmentKind.Tail, family, provider, tooltip)
{
}
