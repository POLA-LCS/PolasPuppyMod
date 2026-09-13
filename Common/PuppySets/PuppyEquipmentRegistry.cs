using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Content.Items.Ears;
using PuppyMod.Content.Items.Tail;

namespace PuppyMod.Common.PuppySets;

public static class PuppyEquipmentRegistry
{
    private static readonly Dictionary<int, PuppyEquipmentDefinition> definitions = new();

    public static void RegisterEars(int itemType, PuppyFamily family, IPuppyEars provider)
    {
        definitions[itemType] = new PuppyEarsDefinition(itemType, family, provider);
    }

    public static void RegisterTail(int itemType, PuppyFamily family, IPuppyTail provider)
    {
        definitions[itemType] = new PuppyTailDefinition(itemType, family, provider);
    }

    public static bool TryGetDefinition(int itemType, out PuppyEquipmentDefinition definition)
    {
        return definitions.TryGetValue(itemType, out definition);
    }

    public static void RegisterDefaults()
    {
        definitions.Clear();
        RegisterEars(ItemID.DogEars, PuppyFamily.Vanilla, new VanillaDogEarsProvider());
        RegisterTail(ItemID.DogTail, PuppyFamily.Vanilla, new VanillaDogTailProvider());

        int reinforcedEarsType = ModContent.ItemType<ReinforcedEarsItem>();
        RegisterEars(reinforcedEarsType, PuppyFamily.Reinforced, ModContent.GetInstance<ReinforcedEarsItem>());

        int reinforcedTailType = ModContent.ItemType<ReinforcedTailItem>();
        RegisterTail(reinforcedTailType, PuppyFamily.Reinforced, ModContent.GetInstance<ReinforcedTailItem>());

        int shinyEarsType = ModContent.ItemType<ShinyEarsItem>();
        RegisterEars(shinyEarsType, PuppyFamily.Shiny, ModContent.GetInstance<ShinyEarsItem>());

        int shinyTailType = ModContent.ItemType<ShinyTailItem>();
        RegisterTail(shinyTailType, PuppyFamily.Shiny, ModContent.GetInstance<ShinyTailItem>());
    }

    public static void Clear()
    {
        definitions.Clear();
    }

    private sealed class VanillaDogEarsProvider : IPuppyEars
    {
        public PuppyEquipmentStats Stats => new(Defense: 0f, PickSpeed: 0.10f);
    }

    private sealed class VanillaDogTailProvider : IPuppyTail
    {
        public PuppyEquipmentStats Stats => new(
            Defense: 0f,
            MoveSpeed: 0.30f,
            AccRunSpeed: 0.45f,
            MaxRunSpeed: 0.30f,
            JumpSpeedBoost: 1.0f);
    }
}
