using PuppyMod.Common.Physics;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Leash;

public abstract class RangedLeashItem : BaseWeaponLeashItem
{
    protected override DamageClass LeashDamageClass => DamageClass.Ranged;

    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.78f,
        Stiffness: 0.16f,
        Damping: 0.60f,
        MaxStretchRatio: 1.32f,
        Curve: LeashElasticityCurve.SmoothRamp,
        PuppyInertia: 1.10f,
        OwnerInertia: 0.18f
    );
}
