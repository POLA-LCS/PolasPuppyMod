using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets.Core;
using PuppyMod.Content.Items.Ears;
using PuppyMod.Content.Items.Tails;

namespace PuppyMod.Common.PuppySets.Definitions;

/// <summary>Central registry mapping item types to their Puppy equipment definitions.</summary>
public static class PuppyEquipmentRegistry
{
    private static readonly Dictionary<int, PuppyEquipmentDefinition> definitions = new();

    public static void RegisterEars(
        int itemType,
        PuppyFamily family,
        IPuppyEarsItem provider,
        PuppyTooltipDefinition tooltip)
    {
        definitions[itemType] = new PuppyEarsDefinition(itemType, family, provider, tooltip);
    }

    public static void RegisterTail(
        int itemType,
        PuppyFamily family,
        IPuppyTailItem provider,
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
                CreateTooltipLine("PuppyEarsStat", "PuppyStat", halveInVanity: true),
                CreateTooltipLine("PuppyEarsFlavor", "PuppyFlavor", halveInVanity: false)));
        RegisterTail(
            ItemID.DogTail,
            PuppyFamily.Vanilla,
            new VanillaDogTailProvider(),
            CreateTooltip(
                "DogTail",
                CreateTooltipLine("PuppyTailStat", "PuppyStat", halveInVanity: true),
                CreateTooltipLine("PuppyTailFlavor", "PuppyFlavor", halveInVanity: false)));

        int reinforcedEarsType = ModContent.ItemType<ReinforcedEarsItem>();
        RegisterEars(
            reinforcedEarsType,
            PuppyFamily.Reinforced,
            ModContent.GetInstance<ReinforcedEarsItem>(),
            CreateTooltip(
                "ReinforcedEarsItem",
                CreateTooltipLine("PuppyReinforcedEarsDefense", "PuppyDefense", halveInVanity: true),
                CreateTooltipLine("PuppyReinforcedEarsKnockback", "PuppyEffect", halveInVanity: true),
                CreateTooltipLine("PuppyReinforcedEarsFlavor", "PuppyFlavor", halveInVanity: false)));

        int reinforcedTailType = ModContent.ItemType<ReinforcedTailItem>();
        RegisterTail(
            reinforcedTailType,
            PuppyFamily.Reinforced,
            ModContent.GetInstance<ReinforcedTailItem>(),
            CreateTooltip(
                "ReinforcedTailItem",
                CreateTooltipLine("PuppyReinforcedTailDefense", "PuppyDefense", halveInVanity: true),
                CreateTooltipLine("PuppyReinforcedTailStat", "PuppyStat", halveInVanity: true),
                CreateTooltipLine("PuppyReinforcedTailFlavor", "PuppyFlavor", halveInVanity: false)));

        int shinyEarsType = ModContent.ItemType<ShinyEarsItem>();
        RegisterEars(
            shinyEarsType,
            PuppyFamily.Shiny,
            ModContent.GetInstance<ShinyEarsItem>(),
            CreateTooltip(
                "ShinyEarsItem",
                CreateTooltipLine("ShinyEarsStat", "PuppyStat", halveInVanity: true),
                CreateTooltipLine("ShinyEarsEffect", "PuppyEffect", halveInVanity: true),
                CreateTooltipLine("ShinyEarsFlavor", "PuppyFlavor", halveInVanity: false)));

        int shinyTailType = ModContent.ItemType<ShinyTailItem>();
        RegisterTail(
            shinyTailType,
            PuppyFamily.Shiny,
            ModContent.GetInstance<ShinyTailItem>(),
            CreateTooltip(
                "ShinyTailItem",
                CreateTooltipLine("ShinyTailStat", "PuppyStat", halveInVanity: true),
                CreateTooltipLine("ShinyTailFlavor", "PuppyFlavor", halveInVanity: false)));
    }

    public static void Clear()
    {
        definitions.Clear();
    }

    private sealed class VanillaDogEarsProvider : IPuppyEarsItem
    {
        public PuppyEquipmentStats Stats => new(Defense: 0f, PickSpeed: 0.10f);
    }

    private sealed class VanillaDogTailProvider : IPuppyTailItem
    {
        // Acceleration above max run speed is clamped in PuppyPlayer.PostUpdateRunSpeeds to avoid sprint dust.
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

    private static PuppyTooltipLineDefinition CreateTooltipLine(
        string lineName,
        string localizationName,
        bool halveInVanity)
    {
        return new PuppyTooltipLineDefinition(lineName, localizationName, halveInVanity);
    }
}
