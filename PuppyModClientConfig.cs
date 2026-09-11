using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PuppyMod;

public class PuppyModClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("PuppySet")]
    [DefaultValue(true)]
    public bool StartAsPuppy;

    [Header("Debug")]
    [DefaultValue(false)]
    public bool BarkDebug;

    [Header("Audio")]
    [Range(0f, 1f)]
    [DefaultValue(0.5f)]
    public float BarkVolume { get; set; } = 0.5f;

    public enum BarkPitchStyle
    {
        FloorShaker = 0,
        Protector = 1,
        BigPup = 2,
        GoodPuppy = 3,
        Wiggly = 4,
        AttentionSeeker = 5,
        Squeak = 6
    }

    [Header("Audio")]
    [DefaultValue(BarkPitchStyle.Wiggly)]
    public BarkPitchStyle BarkPitch { get; set; } = BarkPitchStyle.Wiggly;
}
