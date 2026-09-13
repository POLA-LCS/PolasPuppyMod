namespace PuppyMod.Common.PuppySets;

/// <summary>Defense granted by a set pair, chosen by the pair's placement state.</summary>
public readonly record struct PuppyDefenseEffect(
    int CostumeDefense,
    int FurryDefense,
    int TherianDefense)
{
    public int GetDefense(PuppySetPlacement placement) => placement switch
    {
        PuppySetPlacement.Therian => TherianDefense,
        PuppySetPlacement.Furry => FurryDefense,
        _ => CostumeDefense
    };
}
