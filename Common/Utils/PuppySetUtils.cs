using System;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Content.Items.Ears;

namespace PuppyMod.Common.Utils;

public static class PuppySetUtils
{
    public static short[] EarsItemIDs { get; set; } = [ItemID.DogEars];
    public static short[] TailItemIDs { get; set; } = [ItemID.DogTail];

    private sealed class VanillaDogEarsProvider : IPuppyEars
    {
        public float PickSpeedAccessory => 0.10f;
        public float PickSpeedVanity => PickSpeedAccessory * 0.5f;
    }

    private static readonly IPuppyEars vanillaDogEars = new VanillaDogEarsProvider();

    public static bool IsEars(int itemType) => Array.IndexOf(EarsItemIDs, (short)itemType) >= 0;
    public static bool IsTail(int itemType) => Array.IndexOf(TailItemIDs, (short)itemType) >= 0;

    internal static void RegisterShinyEars()
    {
        short id = (short)ModContent.ItemType<ShinyEarsItem>();
        if (Array.IndexOf(EarsItemIDs, id) < 0)
            EarsItemIDs = EarsItemIDs.Append(id).ToArray();
    }

    public static IPuppyEars GetEarsProvider(int itemType)
    {
        if (itemType == ItemID.DogEars)
            return vanillaDogEars;

        if (!IsEars(itemType))
            return null;

        return ModContent.GetModItem(itemType) as IPuppyEars;
    }
}
