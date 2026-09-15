using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets.Core;

namespace PuppyMod.Common.PuppySets.Definitions;

public sealed class PuppyTailDefinition(
    int itemType,
    PuppyFamily family,
    IPuppyTailItem provider,
    PuppyTooltipDefinition tooltip) : PuppyEquipmentDefinition(itemType, PuppyEquipmentKind.Tail, family, provider, tooltip)
{
}
