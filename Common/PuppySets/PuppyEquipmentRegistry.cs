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

    public static void RegisterEars(
        int itemType,
        PuppyFamily family,
        IPuppyEars provider,
        PuppyTooltipDefinition tooltip)
    {
        definitions[itemType] = new PuppyEarsDefinition(itemType, family, provider, tooltip);
    }

    public static void RegisterTail(
        int itemType,
        PuppyFamily family,
        IPuppyTail provider,
        PuppyTooltipDefinition tooltip)
    {
        definitions[itemType] = new PuppyTailDefinition(itemType, family, provider, tooltip);
    }

    public static bool TryGetDefinition(int itemType, out PuppyEquipmentDefinition definition)
    {
        return definitions.TryGetValue(itemType, out definition);
    }

    public static void RegisterDefaults()
    {
        definitions.Clear();
        RegisterEars(
            ItemID.DogEars,
            PuppyFamily.Vanilla,
            new VanillaDogEarsProvider(),
            CreateTooltip(
                "DogEars",
                Line("PuppyEarsStat", "PuppyStat", halveInVanity: true),
                Line("PuppyEarsFlavor", "PuppyFlavor", halveInVanity: false)));
        RegisterTail(
            ItemID.DogTail,
            PuppyFamily.Vanilla,
            new VanillaDogTailProvider(),
            CreateTooltip(
                "DogTail",
                Line("PuppyTailStat", "PuppyStat", halveInVanity: true),
                Line("PuppyTailFlavor", "PuppyFlavor", halveInVanity: false)));

        int reinforcedEarsType = ModContent.ItemType<ReinforcedEarsItem>();
        RegisterEars(
            reinforcedEarsType,
            PuppyFamily.Reinforced,
            ModContent.GetInstance<ReinforcedEarsItem>(),
            CreateTooltip(
                "ReinforcedEarsItem",
                Line("PuppyReinforcedEarsDefense", "PuppyStat", halveInVanity: false),
                Line("PuppyReinforcedEarsKnockback", "PuppyEffect", halveInVanity: false),
                Line("PuppyReinforcedEarsFlavor", "PuppyFlavor", halveInVanity: false)));

        int reinforcedTailType = ModContent.ItemType<ReinforcedTailItem>();
        RegisterTail(
            reinforcedTailType,
            PuppyFamily.Reinforced,
            ModContent.GetInstance<ReinforcedTailItem>(),
            CreateTooltip(
                "ReinforcedTailItem",
                Line("PuppyReinforcedTailStat", "PuppyStat", halveInVanity: false),
                Line("PuppyReinforcedTailFlavor", "PuppyFlavor", halveInVanity: false)));

        int shinyEarsType = ModContent.ItemType<ShinyEarsItem>();
        RegisterEars(
            shinyEarsType,
            PuppyFamily.Shiny,
            ModContent.GetInstance<ShinyEarsItem>(),
            CreateTooltip(
                "ShinyEarsItem",
                Line("ShinyEarsStat", "PuppyStat", halveInVanity: true),
                Line("ShinyEarsEffect", "PuppyEffect", halveInVanity: true),
                Line("ShinyEarsFlavor", "PuppyFlavor", halveInVanity: false)));

        int shinyTailType = ModContent.ItemType<ShinyTailItem>();
        RegisterTail(
            shinyTailType,
            PuppyFamily.Shiny,
            ModContent.GetInstance<ShinyTailItem>(),
            CreateTooltip(
                "ShinyTailItem",
                Line("ShinyTailStat", "PuppyStat", halveInVanity: true),
                Line("ShinyTailFlavor", "PuppyFlavor", halveInVanity: false)));
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

    private static PuppyTooltipDefinition CreateTooltip(
        string itemLocalizationName,
        params PuppyTooltipLineDefinition[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            PuppyTooltipLineDefinition line = lines[i];
            lines[i] = new PuppyTooltipLineDefinition(
                line.LineName,
                $"Mods.PuppyMod.Items.{itemLocalizationName}.{line.LocalizationKey}",
                line.HalveInVanity);
        }

        return new PuppyTooltipDefinition(lines);
    }

    private static PuppyTooltipLineDefinition Line(
        string lineName,
        string localizationName,
        bool halveInVanity)
    {
        return new PuppyTooltipLineDefinition(lineName, localizationName, halveInVanity);
    }
}
