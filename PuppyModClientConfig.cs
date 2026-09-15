using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PuppyMod;

public class PuppyModClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("Misc")]
    [DefaultValue(true)]
    public bool StartAsPuppy { get; set; } = true;

    [Slider, DrawTicks]
    [DefaultValue(DogTransformationSkin.Beagle)]
    public DogTransformationSkin TransformationSkin { get; set; } = DogTransformationSkin.Beagle;

    [Header("Barking")]
    [Range(0f, 1f)]
    [DefaultValue(0.5f)]
    public float BarkVolume { get; set; } = 0.5f;

    [Range(0f, 1f)]
    [DefaultValue(0.5f)]
    public float OtherBarkVolume { get; set; } = 0.5f;

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