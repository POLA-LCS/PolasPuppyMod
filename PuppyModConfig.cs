using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace PuppyMod;

public class PuppyModConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("PuppySet")]
    [DefaultValue(true)]
    public bool StartingPuppySet;
}