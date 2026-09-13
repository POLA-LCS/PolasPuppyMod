using PuppyMod.Common.Physics;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Leashes;

public abstract class SummonLeashItem : BaseWeaponLeashItem
{
    protected override DamageClass LeashDamageClass => DamageClass.SummonMeleeSpeed;

    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.82f,
        Stiffness: 0.18f,
        Damping: 0.62f,
        MaxStretchRatio: 1.28f,
        Curve: LeashElasticityCurve.PasitoAPasito,
        PuppyInertia: 1.15f,
        OwnerInertia: 0.19f
    );
}
