namespace PuppyMod.Common.PuppySets;

public sealed class PuppyTooltipLineDefinition
{
    public PuppyTooltipLineDefinition(string lineName, string localizationKey, bool halveInVanity)
    {
        LineName = lineName;
        LocalizationKey = localizationKey;
        HalveInVanity = halveInVanity;
    }

    public string LineName { get; }
    public string LocalizationKey { get; }
    public bool HalveInVanity { get; }
}
