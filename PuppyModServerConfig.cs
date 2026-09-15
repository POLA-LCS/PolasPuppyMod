using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PuppyMod;

public class PuppyModServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("Misc")]
    [DefaultValue(true)]
    public bool EnableStartingPuppies { get; set; } = true;

    [Header("Bark")]
    [DefaultValue(true)]
    public bool BarkEnabled { get; set; } = true;

    [Range(0, 100)]
    [Increment(5)]
    [DefaultValue(75)]
    public int BarkRangeTiles { get; set; } = 75;
}
