using PuppyMod.Common.Interfaces;

namespace PuppyMod.Common.PuppySets;

public sealed class PuppyEarsDefinition(
    int itemType,
    PuppyFamily family,
    IPuppyEarsItem provider,
    PuppyTooltipDefinition tooltip) : PuppyEquipmentDefinition(itemType, PuppyEquipmentKind.Ears, family, provider, tooltip)
{
}
