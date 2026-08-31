using Microsoft.Xna.Framework;

namespace PuppyMod.Common.Constants;

public static class PuppyConstants
{
    public const float TileToPixels = 16f;
    public const int MaxLeashTiles = 15;
    public const float MaxLeashPixels = MaxLeashTiles * TileToPixels;

    public const float PuppyPull = 0.10f;
    public const float OwnerPull = 0.018f;
    public const float PullDivisor = 8f;

    public const int ClickSignalTicks = 10;
    public const int BarkCooldownTicks = 20;
    public const int DoubleTapWindow = 18;

    public const int GoodPuppyLifeRegen = 14;
    public const float GoodPuppyMoveSpeed = 0.6f;

    public static readonly Color RopeColor = new(193, 154, 107);
    public const string RopeTexturePath = "Terraria/Images/Chain";

    public const float LeashPenaltyUseTimeMult = 1.45f;
    public const float LeashPenaltyDamageMult = 0.65f;
    public const float LeashPenaltyKnockMult = 0.7f;
}
