using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Content.Items.Ears;
using PuppyMod.Content.Items.Tails;

namespace PuppyMod.Common.PuppySets;

/// <summary>
/// Central registry for Puppy equipment. Research note (tModLoader best practice):
/// For accessories/armor that must give halved vanity (2 functional / 1 vanity), defense cannot be done correctly via Item.defense alone (vanity gives 0 automatically).
/// Use Player.statDefense in PostUpdateEquips via central PuppyEquipmentStats with vanity multiplier (see PuppyPlayer.ApplyEquipmentDefense), as currently done,
/// and keep defense tooltip via custom localization ("2 defense") rather than relying on Item.defense duplicate line.
/// For knockback/movement/jump, modify Player fields in UpdateAccessory/PostUpdateRunSpeeds etc., not Item fields.
/// </summary>
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
        // Note: AccRunSpeed (0.45) > MaxRunSpeed (0.30) would normally trigger Hermes sprint dust when acc > max.
        // PuppyPlayer.PostUpdateRunSpeeds neutralizes this by setting accRunSpeed = maxRunSpeed after applying stats, preserving movement without dust.
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
