using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace PuppyMod;

public class PuppyModServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("PuppySet")]
    [DefaultValue(true)]
    public bool EnableStartingPuppies;
}
