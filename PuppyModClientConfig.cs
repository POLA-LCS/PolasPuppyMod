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

    [Slider, DrawTicks]
    [DefaultValue(BarkPitchStyle.Wiggly)]
    public BarkPitchStyle BarkPitch { get; set; } = BarkPitchStyle.Wiggly;
}

public enum BarkPitchStyle
{
    FloorShaker,
    Protector,
    BigPup,
    GoodPuppy,
    Wiggly,
    AttentionSeeker,
    Squeak
}