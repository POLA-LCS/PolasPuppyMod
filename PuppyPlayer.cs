using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Content.Buffs.GoodPuppy;
using System.Collections.Generic;
using System;

namespace PuppyMod;

public class PuppyPlayer : ModPlayer
{
    public static class BarksArray
    {
        private static SoundStyle LoadPuppySound(string name) =>
            new($"PuppyMod/Assets/Barks/{name}") { Pitch = 0.5f, PitchVariance = 0.5f };

        private static readonly SoundStyle[] barks = [
            LoadPuppySound("growl"),
            LoadPuppySound("growl_woof"),
            LoadPuppySound("woof"),
            LoadPuppySound("woof2"),
        ];

        public static SoundStyle Get(int index)
        {
            if (index < 0 || index >= barks.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return barks[index];
        }

        public static SoundStyle GetRandom(int offset = 1)
        {
            if (offset < 0 || offset >= barks.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            return barks[Main.rand.Next(offset, barks.Length)];
        }
    }

    public enum PuppyState
    {
        Human = 0,
        Furry = 1,
        Therian = 2,
    }

    private int doubleTapUpTimer = 0;
    private bool prevControlUp = false;
    private int barkCooldown = 0;

    public PuppyState HowPuppy { get; private set; } = PuppyState.Human;
    public bool IsPuppy => HowPuppy != PuppyState.Human;
    public bool IsGoodPuppyHappy => Player.HasBuff(ModContent.BuffType<GoodPuppyBuff>());

    private bool IsWearingPuppyEars => Player.armor[10].type == ItemID.DogEars;
    private bool HasAccesory(short item, bool accessory = true, bool vanity = true)
    {
        int extra = Player.GetAmountOfExtraAccessorySlotsToShow();

        if (accessory)
        {
            for (int i = 3; i < 10 + extra && i < Player.armor.Length; i++)
            {
                if (Player.armor[i].type == item)
                    return true;
            }
            return false;
        }
        if (vanity)
        {
            for (int i = 13; i < Player.armor.Length; i++)
            {
                if (Player.armor[i].type == item)
                    return true;
            }
        }

        return false;
    }

    private short[] HasAccesories(short[] items, bool accessory = true, bool vanity = true)
    {
        var hasItems = new List<short>(items.Length);

        if (accessory)
        {
            foreach (var item in items)
            {
                if (HasAccesory(item, vanity: false))
                {
                    hasItems.Add(item);
                }
            }
        }
        if (vanity)
        {
            foreach (var item in items)
            {
                if (HasAccesory(item, accessory: false))
                {
                    hasItems.Add(item);
                }
            }
        }
        return items;
    }

    private PuppyState GetPuppyTailState()
    {
        if (HasAccesory(ItemID.DogTail, vanity: false))
        {
            return PuppyState.Therian;
        }
        if (HasAccesory(ItemID.DogTail, accessory: false))
        {
            return PuppyState.Furry;
        }
        return PuppyState.Human;
    }

    public void Bark(SoundStyle sound, bool pitched = false)
    {
        SoundStyle bark = pitched ? sound with { Pitch = MathHelper.Clamp(sound.Pitch + 0.4f, -1f, 1f) } : sound;
        if (Player.whoAmI == Main.myPlayer)
            SoundEngine.PlaySound(bark, Player.Center);
    }

    public void PlayRandomBark(bool forcePitch = false)
    {
        var bark = BarksArray.GetRandom();
        if (forcePitch || IsGoodPuppyHappy)
            Bark(bark, pitched: true);
        else
            Bark(bark);
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        Item ears = new();
        ears.SetDefaults(ItemID.DogEars);
        Item tail = new();
        tail.SetDefaults(ItemID.DogTail);
        return [ears, tail];
    }

    public override void PostUpdateEquips()
    {
        PuppyState tailState = PuppyState.Human;

        if (IsWearingPuppyEars)
            tailState = GetPuppyTailState();

        if (HowPuppy == PuppyState.Human && tailState != PuppyState.Human)
            PlayRandomBark();



        HowPuppy = tailState;
        if (HowPuppy == PuppyState.Therian)
            Player.setBonus = "Therian Puppy Set: Greatly improved mobility and +15% mining speed\nDouble tap UP to bark! Arf!";
        else if (HowPuppy == PuppyState.Furry)
            Player.setBonus = "Puppy Set: Improved mobility and +15% mining speed\nDouble tap UP to bark! Arf!";
    }

    public override void PreUpdate()
    {
        if (barkCooldown > 0)
            barkCooldown--;

        if (IsPuppy && barkCooldown <= 0)
        {
            bool curUp = Player.controlUp;

            if (curUp && !prevControlUp)
            {
                if (doubleTapUpTimer > 0)
                {
                    PlayRandomBark(forcePitch: IsGoodPuppyHappy);
                    doubleTapUpTimer = 0;
                    barkCooldown = 20;
                }
                else
                {
                    doubleTapUpTimer = 18;
                }
            }

            prevControlUp = curUp;
        }
        else if (!Player.controlUp)
        {
            prevControlUp = false;
        }

        if (doubleTapUpTimer > 0)
            doubleTapUpTimer--;
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        if (!IsPuppy)
            return;

        var growl = BarksArray.Get(Main.rand.Next(0, 2)).WithVolumeScale(0.50f);
        Bark(growl);
    }

    public bool CanHearClicker(Player clickerHolder)
    {
        var owner = clickerHolder.GetModPlayer<OwnerPlayer>();
        if (!owner.HasClicked)
            return false;
        float range = owner.ClickRange;
        if (range <= 0f)
            return false;
        return Vector2.DistanceSquared(Player.Center, clickerHolder.Center) <= range * range;
    }

    public override void PostUpdateMiscEffects()
    {
        if (HowPuppy == PuppyState.Human)
            return;

        if (HowPuppy == PuppyState.Therian)
        {
            Player.moveSpeed += 0.3f;
            Player.accRunSpeed += 0.5f;
            Player.maxRunSpeed += 0.3f;
            Player.jumpSpeedBoost += 0.75f;
            Player.pickSpeed -= 0.15f;
        }
        else
        {
            Player.moveSpeed += 0.15f;
            Player.accRunSpeed += 0.2f;
            Player.maxRunSpeed += 0.15f;
            Player.jumpSpeedBoost += 0.3f;
        }

        for (int i = 0; i < Main.player.Length; i++)
        {
            Player other = Main.player[i];
            if (other == null || !other.active || other.dead || other.whoAmI == Player.whoAmI)
                continue;

            if (!CanHearClicker(other))
                continue;

            var owner = other.GetModPlayer<OwnerPlayer>();
            int buffTime = owner.BuffDuration;
            if (buffTime <= 0)
                buffTime = 60;
            Player.AddBuff(ModContent.BuffType<GoodPuppyBuff>(), buffTime);
        }
    }
}
