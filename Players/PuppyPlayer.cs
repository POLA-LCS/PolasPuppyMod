using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Buffs.GoodPuppy;
using System.Collections.Generic;
using System;
using Stubble.Core;

namespace PuppyMod.Players;

public class PuppyPlayer : PolasBasePlayer
{
    public const int BarkCooldownTicks = 25; // *bark!* little breather :3
    public const int DoubleTapWindow = 18; // *tap tap* zoom window :3
    public const float EarsPickAccessory = 0.10f; // digging zoom! *paw paw* :3
    public const float EarsPickVanity = 0.05f; // lil halved diggy :3
    public const float TailMoveAccessory = 0.30f; // zoomies! *wag wag* :3
    public const float TailAccRunAccessory = 0.45f; // speedy paws :3
    public const float TailMaxRunAccessory = 0.30f; // max zoom! :3
    public const float TailJumpAccessory = 1.0f; // boing! :3
    public const float TailMoveVanity = 0.15f; // soft zoom :3
    public const float TailAccRunVanity = 0.22f; // gentle paws :3
    public const float TailMaxRunVanity = 0.15f; // lil zoom :3
    public const float TailJumpVanity = 0.5f; // little boing :3

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
        SoundStyle bark = pitched ? sound with { Pitch = MathHelper.Clamp(sound.Pitch + 0.4f, -1f, 1f) } : sound;
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
                return;
            Bark(Cries.GetRandom().WithVolumeScale(0.60f));
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
            // *arf!* cute set bonus :3
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
