using PuppyMod.Common.Interfaces;

namespace PuppyMod.Common.PuppySets;

public sealed class PuppyTailDefinition : PuppyEquipmentDefinition
{
    public PuppyTailDefinition(
        int itemType,
        PuppyFamily family,
        IPuppyTailItem provider,
        PuppyTooltipDefinition tooltip)
        : base(itemType, PuppyEquipmentKind.Tail, family, provider, tooltip)
    {
    }
}
