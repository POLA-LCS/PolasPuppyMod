namespace PuppyMod.Common.PuppySets;

public readonly record struct PuppyLeashBonusEffect(
    int DefenseBonus,
    float KnockbackMultiplier)
{
    public bool HasEffect => DefenseBonus != 0 || KnockbackMultiplier < 1f;

    public bool IsStrongerThan(PuppyLeashBonusEffect other)
    {
        if (DefenseBonus != other.DefenseBonus)
            return DefenseBonus > other.DefenseBonus;

        return KnockbackMultiplier < other.KnockbackMultiplier;
    }
}
