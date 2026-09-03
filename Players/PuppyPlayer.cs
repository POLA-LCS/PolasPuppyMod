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
    public const float EarsPickAccessory = 0.10f;
    public const float EarsPickVanity = 0.05f;
    public const float TailMoveAccessory = 0.30f;
    public const float TailAccRunAccessory = 0.45f;
    public const float TailMaxRunAccessory = 0.30f;
    public const float TailJumpAccessory = 1.0f;
    public const float TailMoveVanity = 0.15f;
    public const float TailAccRunVanity = 0.22f;
    public const float TailMaxRunVanity = 0.15f;
    public const float TailJumpVanity = 0.5f;

    private int doubleTapUpTimer = 0;
    private bool prevControlUp = false;
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

    public void PlayRandomBark(bool forcePitch = false)
    {
        var bark = BarksArray.GetRandom();
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
        if (doubleTapUpTimer > 0)
            doubleTapUpTimer--;

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
            Player.setBonus = "Double tap to bark! arf! wruuf!";
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

        float pick = Player.pickSpeed;
        float move = Player.moveSpeed;
        float acc = Player.accRunSpeed;
        float max = Player.maxRunSpeed;
        float jump = Player.jumpSpeedBoost;

        string earsSrc = HasDogEarsAccessory ? "Acc" : HasDogEarsVanity ? "Van" : "None";
        string tailSrc = HasDogTailAccessory ? "Acc" : HasDogTailVanity ? "Van" : "None";

        string msg = $"[Bark] (Ears:{earsSrc} Tail:{tailSrc}) pick:{pick:F2} move:{move:F2} accRun:{acc:F2} maxRun:{max:F2} jump:{jump:F2}";
        Main.NewText(msg, Color.Cyan);
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
