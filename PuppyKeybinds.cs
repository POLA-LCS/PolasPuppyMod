using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace PuppyMod;

public static class PuppyKeybinds
{
    public static ModKeybind Transform { get; private set; }

    internal static void Load(Mod mod)
    {
        Transform = KeybindLoader.RegisterKeybind(mod, nameof(Transform), Keys.P);
    }

    internal static void Unload()
    {
        Transform = null;
    }
}
