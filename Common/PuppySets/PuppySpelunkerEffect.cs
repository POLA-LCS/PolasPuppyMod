namespace PuppyMod.Common.PuppySets;

/// <summary>Ore-highlight radii granted by a set pair, chosen by the pair's placement state.</summary>
public readonly record struct PuppySpelunkerEffect(
    int CostumeRadius,
    int FurryRadius,
    int TherianRadius)
{
    public int GetRadius(PuppySetPlacement placement) => placement switch
    {
        PuppySetPlacement.Therian => TherianRadius,
        PuppySetPlacement.Furry => FurryRadius,
        _ => CostumeRadius
    };
}
