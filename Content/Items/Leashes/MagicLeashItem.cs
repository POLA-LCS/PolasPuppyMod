using PuppyMod.Common.Physics;
using Terraria.ModLoader;

namespace PuppyMod.Content.Items.Leashes;

public abstract class MagicLeashItem : BaseWeaponLeashItem
{
    protected override DamageClass LeashDamageClass => DamageClass.Magic;

    public override LeashPhysicsProfile Physics => new(
        SlackRatio: 0.75f,
        Stiffness: 0.14f,
        Damping: 0.70f,
        MaxStretchRatio: 1.42f,
        Curve: LeashElasticityCurve.Elastic,
        PuppyInertia: 1.05f,
        OwnerInertia: 0.22f
    );
}
