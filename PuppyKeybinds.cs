using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace PuppyMod;

public static class PuppyKeybinds
{
    public static ModKeybind Bend { get; private set; }
    public static ModKeybind Scratch { get; private set; }
    public static ModKeybind Emote { get; private set; }

    internal static void Load(Mod mod)
    {
        Bend = KeybindLoader.RegisterKeybind(mod, nameof(Bend), Keys.I);
        Scratch = KeybindLoader.RegisterKeybind(mod, nameof(Scratch), Keys.O);
        Emote = KeybindLoader.RegisterKeybind(mod, nameof(Emote), Keys.P);
    }

    internal static void Unload()
    {
        Bend = null;
        Scratch = null;
        Emote = null;
    }
}
