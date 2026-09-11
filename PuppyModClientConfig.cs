using System;
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
    [Tooltip("Floor-Shaker: -0.75\nProtector: -0.50\nBig Pup: -0.25\nGood Puppy: 0.00\nWiggly: +0.25\nAttention Seaker: +0.50\nSqueak: +0.75")]
    public float BarkVolume { get; set; } = 0.5f;

    [Range(-0.75f, 0.75f)]
    [DefaultValue(0.25f)]
    [Tooltip("How you are going to hear your barking")]
    public float BarkPitch
    {
        get => _barkPitch;
        set => _barkPitch = (float)Math.Round(value * 4f) / 4f;
    }

    private float _barkPitch = 0.25f;
}
