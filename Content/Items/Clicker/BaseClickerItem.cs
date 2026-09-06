using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;
using PuppyMod.Players;
using PuppyMod.Services.Clicker;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PuppyMod.Common.Extensions;
using PuppyMod.Common.Interfaces;

namespace PuppyMod.Content.Items.Clicker;

public abstract class BaseClickerItem : ModItem, IWithRange, ITooltipProvider
{
    public int RangeTiles { get; protected set; } // *clack!* how far your praise reaches :3
    public int UsageCooldown { get; protected set; } // *paw tap* little pause between clicks :3
    public int BuffDuration { get; protected set; } // *good puppy!* zoom time :3
    public static readonly SoundPad Clicks = SoundPad.LoadCategory("Clicks", pitch: 0f, variance: 0.5f, volume: 0.9f);

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.Thrust;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useTurn = true;
        Item.autoReuse = false;
        Item.maxStack = 1;
        Item.UseSound = null;
        Item.consumable = false;
    }

    public override bool CanUseItem(Player player)
    {
        var puppy = player.GetModPlayer<PuppyPlayer>();
        if (puppy.IsPuppy)
            return false;

        var owner = player.GetModPlayer<OwnerPlayer>();
        return owner.CanClick;
    }

    public override bool? UseItem(Player player)
    {
        ClickerService.Trigger(player, RangeTiles * 16f, BuffDuration, UsageCooldown);
        SoundEngine.PlaySound(Clicks.GetRandom(), player.Center);
        return true;
    }

    public virtual IEnumerable<TooltipLine> GetTooltipLines(Mod mod)
    {
        // *clack clack* cute praises for good puppies! :3
        yield return new TooltipLine(mod, "ClickerRange", $"{RangeTiles} tiles") { OverrideColor = new Color(193, 154, 107) };
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => tooltips.ApplyTooltips(Mod, this);
}
