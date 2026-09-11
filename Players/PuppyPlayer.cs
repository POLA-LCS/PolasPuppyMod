using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Buffs.GoodPuppy;
using System.Collections.Generic;

namespace PuppyMod.Players;

public class PuppyPlayer : PolasBasePlayer
{
    public const int BarkCooldownTicks = 25;
    public const float TailMoveAccessory = 0.30f;
    public const float TailAccRunAccessory = 0.45f;
    public const float TailMaxRunAccessory = 0.30f;
    public const float TailJumpAccessory = 1.0f;
    public const float TailMoveVanity = 0.15f;
    public const float TailAccRunVanity = 0.22f;
    public const float TailMaxRunVanity = 0.15f;
    public const float TailJumpVanity = 0.5f;
    private const float BarkPitchIncrease = 0.3f;
    private const int RandomChanceMax = 100;
    private const int GrowlChanceThreshold = 25;
    private const float PitchClampMin = -1f;
    private const float PitchClampMax = 1f;

    private int barkCooldown = 0;

    // 0 = no item in that slot
    public int DogEarsAccessoryType;
    public int DogEarsVanityType;
    public int DogTailAccessoryType;
    public int DogTailVanityType;

    public bool HasDogEarsAccessory => DogEarsAccessoryType != 0;
    public bool HasDogEarsVanity => DogEarsVanityType != 0;
    public bool HasDogTailAccessory => DogTailAccessoryType != 0;
    public bool HasDogTailVanity => DogTailVanityType != 0;
    public bool HasDogEars => HasDogEarsAccessory || HasDogEarsVanity;
    public bool HasDogTail => HasDogTailAccessory || HasDogTailVanity;
    public bool IsPuppy => HasDogEars && HasDogTail;

    public void Bark(SoundStyle sound, bool pitched = false)
    {
        var config = ModContent.GetInstance<PuppyModClientConfig>();
        float targetPitch = PitchOf(config.BarkPitch) + (pitched ? BarkPitchIncrease : 0f);
        SoundStyle bark = sound with { Pitch = MathHelper.Clamp(targetPitch, PitchClampMin, PitchClampMax), Volume = sound.Volume * config.BarkVolume };
        if (Player.whoAmI == Main.myPlayer)
            SoundEngine.PlaySound(bark, Player.Center);
    }

    public static float PitchOf(BarkPitchStyle style) => style switch
    {
        BarkPitchStyle.FloorShaker => -0.75f,
        BarkPitchStyle.Protector => -0.50f,
        BarkPitchStyle.BigPup => -0.25f,
        BarkPitchStyle.GoodPuppy => 0.00f,
        BarkPitchStyle.Wiggly => 0.25f,
        BarkPitchStyle.AttentionSeeker => 0.50f,
        BarkPitchStyle.Squeak => 0.75f,
        _ => 0.00f
    };

    public static readonly SoundPad Barks = SoundPad.LoadCategory("PuppySounds/woof").AppendSpecific("PuppySounds", ["growl_woof"]);
    public static readonly SoundPad Cries = SoundPad.LoadCategory("PuppySounds/cry");
    public static readonly SoundPad Growls = SoundPad.LoadCategory("PuppySounds/growl");

    public void PlayRandomBark(bool forcePitch = false)
    {
        var bark = Barks.GetRandom();
        bool isGoodPuppy = Player.HasBuff(ModContent.BuffType<GoodPuppyBuff>());
        if (forcePitch || isGoodPuppy)
            Bark(bark, pitched: true);
        else
            Bark(bark);

        NotifyEarsBark();
    }

    private void NotifyEarsBark()
    {
        if (DogEarsAccessoryType != 0)
            PuppySetUtils.GetEarsProvider(DogEarsAccessoryType)?.OnBark(Player);
        if (DogEarsVanityType != 0 && DogEarsVanityType != DogEarsAccessoryType)
            PuppySetUtils.GetEarsProvider(DogEarsVanityType)?.OnBark(Player);
    }

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

    public override void ResetEffects()
    {
        DogEarsAccessoryType = 0;
        DogEarsVanityType = 0;
        DogTailAccessoryType = 0;
        DogTailVanityType = 0;
    }

    public override void PostUpdate()
    {
        if (barkCooldown > 0)
            barkCooldown--;
    }

    public override void ArmorSetBonusActivated()
    {
        if (!IsPuppy)
            return;
        if (barkCooldown > 0)
            return;
        PlayRandomBark();
        barkCooldown = BarkCooldownTicks;
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (!IsPuppy)
            return;
        if (Player.dead)
            return;
        modifiers.DisableSound();
        modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) =>
        {
            if (Player.statLife - info.Damage <= 0)
            {
                Bark(Cries.GetRandom());
                return;
            }
            if (Main.rand.Next(RandomChanceMax) < GrowlChanceThreshold)
                Bark(Growls.GetRandom());
            else
                Bark(Cries.GetRandom());
        };
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

    private void UpdatePuppySetFlags()
    {
        GetDogSetLocationTypes(out int earsAcc, out int earsVan, out int tailAcc, out int tailVan);
        if (DogEarsAccessoryType == 0) DogEarsAccessoryType = earsAcc;
        if (DogEarsVanityType == 0) DogEarsVanityType = earsVan;
        if (DogTailAccessoryType == 0) DogTailAccessoryType = tailAcc;
        if (DogTailVanityType == 0) DogTailVanityType = tailVan;
    }

    public override void PostUpdateEquips()
    {
        UpdatePuppySetFlags();
        if (IsPuppy)
        {
            string dir = Main.ReversedUpDownArmorSetBonuses ? "UP" : "DOWN";
            Player.setBonus = $"Puppy bonus: Double tap {dir} to bark, arf!";
        }
    }

    public override void PostUpdateMiscEffects()
    {
        Player.pickSpeed -= GetCurrentEarsPickSpeed();

        float move = 0f, accRun = 0f, maxRun = 0f, jump = 0f;
        if (HasDogTailAccessory)
        {
            move = TailMoveAccessory;
            accRun = TailAccRunAccessory;
            maxRun = TailMaxRunAccessory;
            jump = TailJumpAccessory;
        }
        else if (HasDogTailVanity)
        {
            move = TailMoveVanity;
            accRun = TailAccRunVanity;
            maxRun = TailMaxRunVanity;
            jump = TailJumpVanity;
        }
        Player.moveSpeed += move;
        Player.accRunSpeed += accRun;
        Player.maxRunSpeed += maxRun;
        Player.jumpSpeedBoost += jump;

        HappyIfClicker();
    }

    private float GetCurrentEarsPickSpeed()
    {
        if (DogEarsAccessoryType != 0)
            return PuppySetUtils.GetEarsProvider(DogEarsAccessoryType)?.PickSpeedAccessory ?? 0f;
        if (DogEarsVanityType != 0)
            return PuppySetUtils.GetEarsProvider(DogEarsVanityType)?.PickSpeedVanity ?? 0f;
        return 0f;
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
