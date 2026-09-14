namespace PuppyMod.Common.PuppySets;

public sealed class PuppyTooltipLineDefinition(string lineName, string localizationKey, bool halveInVanity)
{

    public string LineName { get; } = lineName;
    public string LocalizationKey { get; } = localizationKey;
    public bool HalveInVanity { get; } = halveInVanity;
}
