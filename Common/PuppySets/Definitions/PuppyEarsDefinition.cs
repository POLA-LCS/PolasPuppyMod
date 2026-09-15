using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets.Core;

namespace PuppyMod.Common.PuppySets.Definitions;

public sealed class PuppyEarsDefinition(
    int itemType,
    PuppyFamily family,
    IPuppyEarsItem provider,
    PuppyTooltipDefinition tooltip) : PuppyEquipmentDefinition(itemType, PuppyEquipmentKind.Ears, family, provider, tooltip)
{
}
