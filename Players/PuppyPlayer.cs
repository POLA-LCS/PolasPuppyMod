using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Content.Buffs.GoodPuppy;
using System.Collections.Generic;
using System;

namespace PuppyMod.Players;

public class PuppyPlayer : PolasBasePlayer
{
    public const int BarkCooldownTicks = 20;
    public const int DoubleTapWindow = 18;

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

    private bool IsWearingPuppyEars => Player.armor[10].type == ItemID.DogEars;

    private PuppyState GetPuppyTailState()
    {
        var whereTailEquipped = HasEquippedAccessoryVanity(ItemID.DogTail);
        if (whereTailEquipped.HasValue)
        {
            if (whereTailEquipped.Value)
            {
                return PuppyState.Therian;
            }
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
        bool isGoodPuppy = Player.HasBuff(ModContent.BuffType<GoodPuppyBuff>());
        if (forcePitch || isGoodPuppy)
            Bark(bark, pitched: true);
        else
            Bark(bark);
    }

    // ====== Overriding section =========
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        var server = ModContent.GetInstance<PuppyModServerConfig>();
        if (!server.EnableStartingPuppies)
            return [];

        var client = ModContent.GetInstance<PuppyModClientConfig>();
        if (!client.StartAsPuppy)
            return [];

        Item dog_ears = new();
        dog_ears.SetDefaults(ItemID.DogEars);
        Item dog_tail = new();
        dog_tail.SetDefaults(ItemID.DogTail);
        return [dog_ears, dog_tail];
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
            Player.setBonus = "Therian Puppy Set: better Puppy Set and +15% mining speed\nDouble tap UP to bark! Arf!";
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
                    PlayRandomBark();
                    doubleTapUpTimer = 0;
                    barkCooldown = BarkCooldownTicks;
                }
                else
                {
                    doubleTapUpTimer = DoubleTapWindow;
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

        // Dog Set bonus
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

        HappyIfClicker();
    }

    private void HappyIfClicker()
    {
        for (int i = 0; i < Main.player.Length; i++)
        {
            Player other = Main.player[i];
            if (other == null || !other.active || other.dead || other.whoAmI == Player.whoAmI)
                continue;

            if (!CanHearClicker(other))
                continue;

            var owner = other.GetModPlayer<OwnerPlayer>();
            int buffTime = owner.BuffDuration;
            Player.AddBuff(ModContent.BuffType<GoodPuppyBuff>(), buffTime);
        }
    }
}

internal static class BarksArray
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