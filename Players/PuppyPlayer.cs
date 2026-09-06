using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Buffs.GoodPuppy;
using System.Collections.Generic;

namespace PuppyMod.Players;

/// <summary>
/// Provides puppy-specific behavior including ear/tail accessories, barking mechanics, and clicker detection.
/// </summary>
public class PuppyPlayer : PolasBasePlayer
{
    /// <summary>
    /// The number of ticks between when a puppy can bark again after barking.
    /// </summary>
    public const int BarkCooldownTicks = 25;
    /// <summary>
    /// The window of ticks for double-tap detection to trigger barking.
    /// </summary>
    public const int DoubleTapWindow = 18;
    /// <summary>
    /// The pick speed increase from dog ears when used as an accessory.
    /// </summary>
    public const float EarsPickAccessory = 0.10f;
    /// <summary>
    /// The pick speed increase from dog ears when used as a vanity item.
    /// </summary>
    public const float EarsPickVanity = 0.05f;
    /// <summary>
    /// The move speed increase from a dog tail when used as an accessory.
    /// </summary>
    public const float TailMoveAccessory = 0.30f;
    /// <summary>
    /// The acceleration run speed increase from a dog tail when used as an accessory.
    /// </summary>
    public const float TailAccRunAccessory = 0.45f;
    /// <summary>
    /// The maximum run speed increase from a dog tail when used as an accessory.
    /// </summary>
    public const float TailMaxRunAccessory = 0.30f;
    /// <summary>
    /// The jump speed boost from a dog tail when used as an accessory.
    /// </summary>
    public const float TailJumpAccessory = 1.0f;
    /// <summary>
    /// The move speed increase from dog ears when used as a vanity item.
    /// </summary>
    public const float TailMoveVanity = 0.15f;
    /// <summary>
    /// The acceleration run speed increase from a dog tail when used as a vanity item.
    /// </summary>
    public const float TailAccRunVanity = 0.22f;
    /// <summary>
    /// The maximum run speed increase from a dog tail when used as a vanity item.
    /// </summary>
    public const float TailMaxRunVanity = 0.15f;
    /// <summary>
    /// The jump speed boost from a dog tail when used as a vanity item.
    /// </summary>
    public const float TailJumpVanity = 0.5f;
    /// <summary>
    /// The pitch increase applied when barking is pitched up.
    /// </summary>
    private const float BarkPitchIncrease = 0.4f;
    /// <summary>
    /// The maximum value for random chance comparisons.
    /// </summary>
    private const int RandomChanceMax = 100;
    /// <summary>
    /// The chance threshold (out of <see cref="RandomChanceMax"/>) for triggering a growl bark.
    /// </summary>
    private const int GrowlChanceThreshold = 75;
    /// <summary>
    /// The minimum pitch clamp value for sound pitch adjustment.
    /// </summary>
    private const float PitchClampMin = -1f;
    /// <summary>
    /// The maximum pitch clamp value for sound pitch adjustment.
    /// </summary>
    private const float PitchClampMax = 1f;

    private int barkCooldown = 0;

    public bool HasDogEarsAccessory;
    public bool HasDogEarsVanity;
    public bool HasDogTailAccessory;
    public bool HasDogTailVanity;
    public bool HasDogEars => HasDogEarsAccessory || HasDogEarsVanity;
    public bool HasDogTail => HasDogTailAccessory || HasDogTailVanity;
    public bool IsPuppy => HasDogEars && HasDogTail;

    public void Bark(SoundStyle sound, bool pitched = false)
    {
        SoundStyle bark = pitched ? sound with { Pitch = MathHelper.Clamp(sound.Pitch + BarkPitchIncrease, PitchClampMin, PitchClampMax) } : sound;
        if (Player.whoAmI == Main.myPlayer)
            SoundEngine.PlaySound(bark, Player.Center);
    }

    public static readonly SoundPad Barks = SoundPad.LoadCategory("PuppySounds/woof");
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

        TryShowBarkDebug();
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
        HasDogEarsAccessory = false;
        HasDogEarsVanity = false;
        HasDogTailAccessory = false;
        HasDogTailVanity = false;
    }

    public override void PreUpdate()
    {
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
        bool ea = HasDogEarsAccessory, ev = HasDogEarsVanity, ta = HasDogTailAccessory, tv = HasDogTailVanity;
        GetDogSetLocationFlags(out bool earsAcc, out bool earsVan, out bool tailAcc, out bool tailVan);
        HasDogEarsAccessory = ea || earsAcc;
        HasDogEarsVanity = ev || earsVan;
        HasDogTailAccessory = ta || tailAcc;
        HasDogTailVanity = tv || tailVan;
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
        float earPick = 0f;
        if (HasDogEarsAccessory) earPick = EarsPickAccessory;
        else if (HasDogEarsVanity) earPick = EarsPickVanity;
        Player.pickSpeed -= earPick;

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

    private void TryShowBarkDebug()
    {
        if (!ModContent.GetInstance<PuppyModClientConfig>().BarkDebug)
            return;
        if (Player.whoAmI != Main.myPlayer)
            return;
        if (Main.netMode == Terraria.ID.NetmodeID.Server)
            return;

        // *arf!* cute debug for zoomy pups :3
        string earsSrc = HasDogEarsAccessory ? "Acc" : HasDogEarsVanity ? "Van" : "None";
        string tailSrc = HasDogTailAccessory ? "Acc" : HasDogTailVanity ? "Van" : "None";

        string msg = $"*arf!* You're feeling zoomy! Ears:{earsSrc} Tail:{tailSrc} *wag*";
        Main.NewText(msg, Color.Cyan);
    }
}
