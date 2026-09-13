using PuppyMod.Common.Interfaces;

namespace PuppyMod.Common.PuppySets;

public sealed class PuppyEarsDefinition : PuppyEquipmentDefinition
{
    public PuppyEarsDefinition(
        int itemType,
        PuppyFamily family,
        IPuppyEarsItem provider,
        PuppyTooltipDefinition tooltip)
        : base(itemType, PuppyEquipmentKind.Ears, family, provider, tooltip)
    {
    }
}
