using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PuppyMod;

public class PuppyModClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("PuppySet")]
    [DefaultValue(true)]
    public bool StartAsPuppy { get; set; } = true;

    [Header("Audio")]
    [Range(0f, 1f)]
    [DefaultValue(0.5f)]
    public float BarkVolume { get; set; } = 0.5f;

    [Slider, DrawTicks]
    [DefaultValue(BarkPitchStyle.Wiggly)]
    public BarkPitchStyle BarkPitch { get; set; } = BarkPitchStyle.Wiggly;

    [Header("Transformation")]
    [Slider, DrawTicks]
    [DefaultValue(DogTransformationSkin.Beagle)]
    public DogTransformationSkin TransformationSkin { get; set; } = DogTransformationSkin.Beagle;
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