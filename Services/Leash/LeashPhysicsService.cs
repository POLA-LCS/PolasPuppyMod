using System;
using Microsoft.Xna.Framework;
using PuppyMod.Common.Data;
using PuppyMod.Common.Enums;

namespace PuppyMod.Services.Leash;

public readonly record struct LeashForceResult(Vector2 PuppyImpulse, Vector2 OwnerImpulse, float Tension01, bool IsTaut, bool Overstretched);

public static class LeashPhysicsService
{
    public static LeashForceResult Compute(Vector2 puppyCenter, Vector2 ownerCenter, Vector2 puppyVel, Vector2 ownerVel, int rangeTiles, LeashPhysicsProfile profile)
    {
        Vector2 delta = ownerCenter - puppyCenter;
        float distance = delta.Length();

        float slack = profile.SlackDistance(rangeTiles);
        float max = profile.MaxDistance(rangeTiles);
        float elasticLen = profile.ElasticLength(rangeTiles);

        if (distance <= slack || distance < 0.001f || rangeTiles <= 0)
        {
            return new LeashForceResult(Vector2.Zero, Vector2.Zero, 0f, false, false);
        }

        Vector2 dir = delta / Math.Max(distance, 0.001f);

        float stretch = distance - slack;
        float t = MathHelper.Clamp(stretch / Math.Max(1f, elasticLen), 0f, 1f);
        bool overstretched = distance > max;

        float shaped = EvaluateCurve(t, profile.Curve);

        float springForce = shaped * profile.Stiffness * stretch;

        Vector2 relVel = puppyVel - ownerVel;
        float vRel = Vector2.Dot(relVel, dir);

        float dampingForce = profile.EffectiveDamping * vRel;

        float net = springForce - dampingForce;
        if (net < 0f)
            net = 0f;

        if (overstretched)
        {
            net *= 6f;
        }

        Vector2 puppyImpulse = dir * net * profile.PuppyInertia;
        Vector2 ownerImpulse = -dir * net * profile.OwnerInertia;

        float tension01 = MathHelper.Clamp(shaped, 0f, 1f);
        if (overstretched)
            tension01 = 1f;

        bool isTaut = true;
        return new LeashForceResult(puppyImpulse, ownerImpulse, tension01, isTaut, overstretched);
    }

    [Obsolete("Use Compute with int rangeTiles - range is in tiles, conversion to pixels (*16) happens inside physics")]
    public static LeashForceResult Compute(Vector2 puppyCenter, Vector2 ownerCenter, Vector2 puppyVel, Vector2 ownerVel, float rangePixels, LeashPhysicsProfile profile)
    {
        Vector2 delta = ownerCenter - puppyCenter;
        float distance = delta.Length();
        float slack = profile.SlackDistance(rangePixels);
        float max = profile.MaxDistance(rangePixels);
        float elasticLen = profile.ElasticLength(rangePixels);
        if (distance <= slack || distance < 0.001f || rangePixels <= 0f)
        {
            return new LeashForceResult(Vector2.Zero, Vector2.Zero, 0f, false, false);
        }
        Vector2 dir = delta / Math.Max(distance, 0.001f);
        float stretch = distance - slack;
        float t = MathHelper.Clamp(stretch / Math.Max(1f, elasticLen), 0f, 1f);
        bool overstretched = distance > max;
        float shaped = EvaluateCurve(t, profile.Curve);
        float springForce = shaped * profile.Stiffness * stretch;
        Vector2 relVel = puppyVel - ownerVel;
        float vRel = Vector2.Dot(relVel, dir);
        float dampingForce = profile.EffectiveDamping * vRel;
        float net = springForce - dampingForce;
        if (net < 0f)
            net = 0f;
        if (overstretched)
            net *= 6f;
        Vector2 puppyImpulse = dir * net * profile.PuppyInertia;
        Vector2 ownerImpulse = -dir * net * profile.OwnerInertia;
        float tension01 = MathHelper.Clamp(shaped, 0f, 1f);
        if (overstretched)
            tension01 = 1f;
        return new LeashForceResult(puppyImpulse, ownerImpulse, tension01, true, overstretched);
    }

    public static float EvaluateCurve(float t, LeashElasticityCurve curve)
    {
        t = MathHelper.Clamp(t, 0f, 1f);
        switch (curve)
        {
            case LeashElasticityCurve.Linear:
                return t;
            case LeashElasticityCurve.SmoothRamp:
                return t * t * (3f - 2f * t);
            case LeashElasticityCurve.SteepStep:
                return t * t * t * (t * (6f * t - 15f) + 10f);
            case LeashElasticityCurve.ElasticBounce:
                return ElasticOut(t);
            case LeashElasticityCurve.Exponential:
                return t * t;
            default:
                return t;
        }
    }

    private static float ElasticOut(float t)
    {
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        const float p = 0.3f;
        return MathF.Pow(2f, -10f * t) * MathF.Sin((t - p / 4f) * (2f * MathF.PI) / p) + 1f;
    }
}
