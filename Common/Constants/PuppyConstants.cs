using Microsoft.Xna.Framework;

namespace PuppyMod.Common.Constants;

public static class PuppyConstants
{
    public const float TileToPixels = 16f;
    public const int MaxLeashTiles = 15;
    public const float MaxLeashPixels = MaxLeashTiles * TileToPixels;

    public const int ClickSignalTicks = 10;
    public const int BarkCooldownTicks = 20;
    public const int DoubleTapWindow = 18;

    public static readonly Color RopeColor = new(193, 154, 107);
    public const string RopeTexturePath = "Terraria/Images/Chain";

    public const float LeashPenaltyUseTimeMult = 1.45f;
    public const float LeashPenaltyDamageMult = 0.65f;
    public const float LeashPenaltyKnockMult = 0.7f;
}
