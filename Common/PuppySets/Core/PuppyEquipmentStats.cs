namespace PuppyMod.Common.PuppySets.Core;

/// <summary>
/// Immutable additive contribution from one equipped Puppy item, or the aggregate of all
/// recognized entries in a snapshot. Values are full-strength functional-slot values;
/// <see cref="PuppyEquipmentEntry.ValueMultiplier"/> applies vanity scaling.
/// </summary>
public readonly record struct PuppyEquipmentStats(
    float Defense = 0f,
    float PickSpeed = 0f,
    float MoveSpeed = 0f,
    float AccRunSpeed = 0f,
    float MaxRunSpeed = 0f,
    /// <summary>
    /// Fractional bonus applied to <c>Player.runAcceleration</c> as a multiplier, so 0.30f means
    /// <c>runAcceleration *= 1.30f</c> and 0f means no change. This is not a raw runAcceleration
    /// amount: the vanilla base is 0.08f and vanilla consumes the field as a multiplier target.
    /// </summary>
    float RunAccelerationPercent = 0f,
    float JumpSpeedBoost = 0f,
    float MeleeKnockbackAdditive = 0f,
    float SummonKnockbackFlat = 0f)
{
    public PuppyEquipmentStats Scale(float multiplier) => new(
        Defense * multiplier,
        PickSpeed * multiplier,
        MoveSpeed * multiplier,
        AccRunSpeed * multiplier,
        MaxRunSpeed * multiplier,
        RunAccelerationPercent * multiplier,
        JumpSpeedBoost * multiplier,
        MeleeKnockbackAdditive * multiplier,
        SummonKnockbackFlat * multiplier);

    public static PuppyEquipmentStats operator +(PuppyEquipmentStats left, PuppyEquipmentStats right) => new(
        left.Defense + right.Defense,
        left.PickSpeed + right.PickSpeed,
        left.MoveSpeed + right.MoveSpeed,
        left.AccRunSpeed + right.AccRunSpeed,
        left.MaxRunSpeed + right.MaxRunSpeed,
        left.RunAccelerationPercent + right.RunAccelerationPercent,
        left.JumpSpeedBoost + right.JumpSpeedBoost,
        left.MeleeKnockbackAdditive + right.MeleeKnockbackAdditive,
        left.SummonKnockbackFlat + right.SummonKnockbackFlat);
}
