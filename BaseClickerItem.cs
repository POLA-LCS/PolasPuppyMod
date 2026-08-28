using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod;

public abstract class BaseClickerItem : ModItem
{
    public int TileRange { get; protected set; } // tile amount
    public int UsageCooldown { get; protected set; } // amount of ticks
    public int BuffDuration { get; protected set; } // amount of ticks (60 = 1 second)
    public static class ClicksArray
    {
        private static SoundStyle LoadPuppySound(string name) =>
            new($"PuppyMod/Assets/Clicks/{name}") { Volume = 0.9f, PitchVariance = 0.5f };

        private static readonly SoundStyle[] clicks = [
            LoadPuppySound("clicker1"),
            LoadPuppySound("clicker2"),
            LoadPuppySound("clicker3"),
        ];

        public static SoundStyle Get(int index)
        {
            if (index < 0 || index >= clicks.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));
            return clicks[index];
        }

        public static SoundStyle GetRandom(int offset = 0)
        {
            if (offset < 0 || offset >= clicks.Length)
                throw new System.ArgumentOutOfRangeException(nameof(offset));
            return clicks[Main.rand.Next(offset, clicks.Length)];
        }
    }

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
        var ownerPlayer = player.GetModPlayer<OwnerPlayer>();
        float rangeInPixels = TileRange * 16f;
        ownerPlayer.TriggerClick(rangeInPixels, BuffDuration, UsageCooldown);
        SoundEngine.PlaySound(ClicksArray.GetRandom(), player.Center);
        return true;
    }
}
