using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using MorphAPI.Core;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Buffs;
using PuppyMod.Content.Items.Ears;
using PuppyMod.Content.Items.Tails;
using PuppyMod.Content.Transformations;
using PuppyMod.Services.Leash;
using PuppyMod.Services.PuppySets;

namespace PuppyMod.Players;

public class PuppyPlayer : ModPlayer
{
    /// <summary>Bark cooldown in ticks (25 = ~0.42s at 60 TPS) prevents spam between set-bonus bark and hurt barks.</summary>
    public const int BarkCooldownTicks = 25;
    /// <summary>Pitch increase when forced/puppy buff active (+0.3).</summary>
    private const float BarkPitchIncrease = 0.3f;
    private const int RandomChanceMax = 100;
    private const int GrowlChanceThreshold = 25;
    private const float PitchClampMin = -1f;
    private const float PitchClampMax = 1f;

    private int _barkCooldown = 0;
    private DogEmote _emoteChoice = DogEmote.None;
    private PuppyEquipmentSnapshot _equipmentSnapshot = PuppyEquipmentSnapshot.Empty;
    private PuppyEquipmentResolution _equipmentResolution = PuppyEquipmentResolution.Empty;
    private bool _shinyTailFunctional;
    private bool _shinyTailVanity;
    private bool _shinyEarsFunctional;
    private bool _shinyEarsVanity;

    public PuppyEquipmentSnapshot EquipmentSnapshot => _equipmentSnapshot;
    public PuppyEquipmentResolution EquipmentResolution => _equipmentResolution;
    public bool IsPuppy => _equipmentResolution.IsPuppy;

    internal void EnableShinyTail(bool isVanity)
    {
        if (isVanity)
            _shinyTailVanity = true;
        else
            _shinyTailFunctional = true;
    }

    internal void EnableShinyEars(bool isVanity)
    {
        if (isVanity)
            _shinyEarsVanity = true;
        else
            _shinyEarsFunctional = true;
    }

    public void Bark(SoundStyle sound, bool pitched = false)
    {
        var config = ModContent.GetInstance<PuppyModClientConfig>();
        // Apply client-chosen volume/pitch; GoodPuppy buff +0.3 pitch
        float targetPitch = GetPitch(config.BarkPitch) + (pitched ? BarkPitchIncrease : 0f);
        SoundStyle bark = sound with { Pitch = MathHelper.Clamp(targetPitch, PitchClampMin, PitchClampMax), Volume = sound.Volume * config.BarkVolume };
        if (Player.whoAmI == Main.myPlayer)
            SoundEngine.PlaySound(bark, Player.Center);
    }

    public static float GetPitch(BarkPitchStyle style) => style switch
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
        if (Barks.Count == 0)
            return;
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
        var notifiedTypes = new HashSet<int>();
        foreach (PuppyEquipmentEntry entry in _equipmentResolution.Ears)
        {
            if (!notifiedTypes.Add(entry.ItemType))
                continue;

            entry.EarsProvider?.OnBark(Player);
        }
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        var server = ModContent.GetInstance<PuppyModServerConfig>();
        if (!server.EnableStartingPuppies)
            return [];

        var client = ModContent.GetInstance<PuppyModClientConfig>();
        if (!client.StartAsPuppy)
            return [];

        Item dogEars = new();
        dogEars.SetDefaults(ItemID.DogEars);
        Item dogTail = new();
        dogTail.SetDefaults(ItemID.DogTail);
        return [dogEars, dogTail];
    }

    public override void ResetEffects()
    {
        _equipmentSnapshot = PuppyEquipmentSnapshot.Empty;
        _equipmentResolution = PuppyEquipmentResolution.Empty;
        _shinyTailFunctional = false;
        _shinyTailVanity = false;
        _shinyEarsFunctional = false;
        _shinyEarsVanity = false;
    }

    public override void PreUpdateMovement()
    {
        if (!_shinyTailFunctional && !_shinyTailVanity)
            return;

        int hoverDuration = _shinyTailFunctional
            ? ShinyTailItem.FunctionalHoverDuration
            : ShinyTailItem.VanityHoverDuration;

        // Cap carpetTime; vanilla initializes to 300 ticks.
        if (Player.carpetFrame >= 0 && Player.carpetTime > hoverDuration)
            Player.carpetTime = hoverDuration;

        if (Main.netMode == NetmodeID.Server)
            return;
        if (Player.carpetFrame < 0)
            return;

        SpawnShinyTailPlatformDust();
    }

    public override void PostUpdate()
    {
        if (_barkCooldown > 0)
            _barkCooldown--;
        // ShinyEars light stays an individual effect: full functional, half vanity.
        if (_shinyEarsFunctional || _shinyEarsVanity)
        {
            bool isVanity = !_shinyEarsFunctional && _shinyEarsVanity;
            ShinyEarsItem.EmitLightForPlayer(Player, isVanity);
        }

        // Ore sight belongs to the Shiny pair bonus and only exists while the matching pair is selected.
        PuppySpelunkerService.Apply(Player);
    }

    public override void ArmorSetBonusActivated()
    {
        if (!IsPuppy)
            return;
        if (_barkCooldown > 0)
            return;
        PlayRandomBark();
        _barkCooldown = BarkCooldownTicks;
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (PuppyLeashBonusService.TryGetKnockbackMultiplier(Player, out float knockbackMultiplier))
            modifiers.Knockback *= knockbackMultiplier;

        if (!IsPuppy)
            return;
        if (Player.dead)
            return;
        modifiers.DisableSound();
        modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) =>
        {
            // Hurt cries share the bark cooldown so they can't overlap a set-bonus bark.
            if (_barkCooldown > 0)
                return;
            if (Player.statLife - info.Damage <= 0)
            {
                if (Cries.Count != 0)
                {
                    Bark(Cries.GetRandom());
                    _barkCooldown = BarkCooldownTicks;
                }
                return;
            }
            if (Main.rand.Next(RandomChanceMax) < GrowlChanceThreshold)
            {
                if (Growls.Count != 0)
                {
                    Bark(Growls.GetRandom());
                    _barkCooldown = BarkCooldownTicks;
                }
            }
            else
            {
                if (Cries.Count != 0)
                {
                    Bark(Cries.GetRandom());
                    _barkCooldown = BarkCooldownTicks;
                }
            }
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

    public override void PostUpdateEquips()
    {
        _equipmentSnapshot = PuppyEquipmentScanner.Scan(Player);
        _equipmentResolution = PuppyEquipmentResolver.Resolve(_equipmentSnapshot);
        ApplyEquipmentDefense();
        ApplyEquipmentKnockback();
        // Attached pair defense is applied with the other equipment stats.
        PuppyLeashBonusService.ApplyDefenseForPlayer(Player);
        // Reset setBonus each tick before appending the active lines.
        Player.setBonus = string.Empty;
        if (IsPuppy)
        {
            foreach (string bonusText in PuppySetBonusText.GetActiveLines(_equipmentResolution, forTooltip: false))
            {
                Player.setBonus = AppendSetBonusLine(Player.setBonus, bonusText);
            }
        }

        UpdateTransformation();
    }

    private void UpdateTransformation()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        bool mountEquipped = !Player.miscEquips[3].IsAir;
        bool shouldTransform = IsPuppy && !mountEquipped && !Player.mount.Active;

        if (shouldTransform && !Player.HasMorph<DogTransformationMorph>())
            Player.SetMorph(new DogTransformationMorph());
        else if (!shouldTransform && Player.HasMorph<DogTransformationMorph>())
            Player.Unmorph();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (!Player.HasMorph<DogTransformationMorph>())
            return;

        if (PuppyKeybinds.Emote.JustPressed)
            _emoteChoice = Main.rand.NextBool() ? DogEmote.Bend : DogEmote.Scratch;

        DogTransformationMorph morph = Player.GetMorph<DogTransformationMorph>();

        if (!morph.CanEmote(Player))
            return;

        if (PuppyKeybinds.Bend.Current)
            morph.PlayEmote(Player, DogEmote.Bend);
        else if (PuppyKeybinds.Scratch.Current)
            morph.PlayEmote(Player, DogEmote.Scratch);
        else if (PuppyKeybinds.Emote.Current)
            morph.PlayEmote(Player, _emoteChoice);
    }

    private static string AppendSetBonusLine(string existing, string line)
    {
        if (string.IsNullOrEmpty(line))
            return existing;
        // Exact line check via split instead of substring Contains to avoid fragile false positives.
        if (!string.IsNullOrEmpty(existing) && existing.Split('\n').Contains(line))
            return existing;
        if (string.IsNullOrWhiteSpace(existing))
            return line;
        return existing + "\n" + line;
    }

    public override void PostUpdateMiscEffects()
    {
        PuppyEquipmentStats stats = _equipmentResolution.Stats;
        Player.pickSpeed -= stats.PickSpeed;
        Player.jumpSpeedBoost += stats.JumpSpeedBoost;

        ApplyClickerPraise();
    }

    public override void PostUpdateRunSpeeds()
    {
        PuppyEquipmentStats stats = _equipmentResolution.Stats;
        Player.moveSpeed += stats.MoveSpeed;
        Player.accRunSpeed += stats.AccRunSpeed;
        Player.maxRunSpeed += stats.MaxRunSpeed;

        // Clamp acceleration to max run speed for puppies so sprint dust never triggers.
        if (IsPuppy && Player.accRunSpeed > Player.maxRunSpeed)
            Player.accRunSpeed = Player.maxRunSpeed;
    }

    private void ApplyEquipmentDefense()
    {
        int defense = (int)MathF.Round(_equipmentResolution.Stats.Defense, MidpointRounding.AwayFromZero);
        if (defense != 0)
            Player.statDefense += defense;
    }

    private void ApplyEquipmentKnockback()
    {
        PuppyEquipmentStats stats = _equipmentResolution.Stats;
        Player.GetKnockback(DamageClass.Melee) += stats.MeleeKnockbackAdditive;
        Player.GetKnockback(DamageClass.Summon).Flat += stats.SummonKnockbackFlat;
    }

    private void ApplyClickerPraise()
    {
        if (!IsPuppy)
            return;

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

    private void SpawnShinyTailPlatformDust()
    {
        if (Main.dedServ || Main.netMode == NetmodeID.Server)
            return;

        Vector2 platformCenter = Player.position + new Vector2(Player.width * 0.5f, Player.height + 2f);
        for (int i = 0; i < 4; i++)
        {
            float offsetX = (i - 1.5f) * 10f + Main.rand.NextFloat(-2f, 2f);
            Vector2 position = platformCenter + new Vector2(offsetX, Main.rand.NextFloat(-1f, 1f));
            Dust dust = Dust.NewDustPerfect(
                position,
                DustID.YellowStarDust,
                new Vector2(0f, -0.15f),
                0,
                default,
                Main.rand.NextFloat(0.8f, 1.2f));
            dust.noGravity = true;
            dust.fadeIn = 0.2f;
        }
    }

}
