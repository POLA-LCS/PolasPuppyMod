using PuppyMod.Common.Physics;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Leashes;

public abstract class MeleeLeashItem : BaseWeaponLeashItem
{
    protected override DamageClass LeashDamageClass => DamageClass.Melee;

    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.85f,
        Stiffness: 0.22f,
        Damping: 0.65f,
        MaxStretchRatio: 1.18f,
        Curve: LeashElasticityCurve.Linear,
        PuppyInertia: 1.20f,
        OwnerInertia: 0.20f
    );
}
