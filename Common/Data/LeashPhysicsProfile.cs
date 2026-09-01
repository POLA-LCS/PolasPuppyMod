using System;
using Microsoft.Xna.Framework;
using PuppyMod.Common.Enums;
namespace PuppyMod.Common.Data;

public readonly record struct LeashPhysicsProfile(
    float SlackRatio = 0.80f,
    float Stiffness = 0.12f,
    float Damping = 0.55f,
    float MaxStretchRatio = 1.0f,
    LeashElasticityCurve Curve = LeashElasticityCurve.Linear,
    float PuppyInertia = 1.00f,
    float OwnerInertia = 0.18f)
{
    public float SlackDistance(int rangeTiles) => rangeTiles * 16f * MathHelper.Clamp(SlackRatio, 0f, 0.98f);
    public float MaxDistance(int rangeTiles) => rangeTiles * 16f * Math.Max(1.01f, MaxStretchRatio);
    public float ElasticLength(int rangeTiles) => Math.Max(1f, MaxDistance(rangeTiles) - SlackDistance(rangeTiles));
    public float EffectiveDamping => Damping * 2f * MathF.Sqrt(Math.Max(0.001f, Stiffness));
}
