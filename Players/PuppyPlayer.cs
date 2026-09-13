using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.PuppySets;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Buffs.GoodPuppy;
using PuppyMod.Content.Items.Tail;
using PuppyMod.Services.Leash;
using System.Collections.Generic;

namespace PuppyMod.Players;

public class PuppyPlayer : ModPlayer
{
    public const int BarkCooldownTicks = 25;
    private const float BarkPitchIncrease = 0.3f;
    private const int RandomChanceMax = 100;
    private const int GrowlChanceThreshold = 25;
    private const float PitchClampMin = -1f;
    private const float PitchClampMax = 1f;

    private int barkCooldown = 0;
    private PuppyEquipmentSnapshot equipmentSnapshot = PuppyEquipmentSnapshot.Empty;
    private PuppyEquipmentResolution equipmentResolution = PuppyEquipmentResolution.Empty;
    private bool shinyTailFunctional;
    private bool shinyTailVanity;

    public PuppyEquipmentSnapshot EquipmentSnapshot => equipmentSnapshot;
    public PuppyEquipmentResolution EquipmentResolution => equipmentResolution;
    public bool IsPuppy => equipmentResolution.IsPuppy;

    internal void EnableShinyTail(bool isVanity)
    {
        if (isVanity)
            shinyTailVanity = true;
        else
            shinyTailFunctional = true;
    }

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
        var notifiedTypes = new HashSet<int>();
        foreach (PuppyEquipmentEntry entry in equipmentResolution.Ears)
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

        Item dog_ears = new();
        dog_ears.SetDefaults(ItemID.DogEars);
        Item dog_tail = new();
        dog_tail.SetDefaults(ItemID.DogTail);
        return [dog_ears, dog_tail];
    }

    public override void ResetEffects()
    {
        equipmentSnapshot = PuppyEquipmentSnapshot.Empty;
        equipmentResolution = PuppyEquipmentResolution.Empty;
        shinyTailFunctional = false;
        shinyTailVanity = false;
    }

    public override void PreUpdateMovement()
    {
        if (!shinyTailFunctional && !shinyTailVanity)
            return;

        int hoverDuration = shinyTailFunctional
            ? ShinyTailItem.FunctionalHoverDuration
            : ShinyTailItem.VanityHoverDuration;

        // Vanilla CarpetMovement runs immediately before this hook and starts with 300 ticks.
        // Cap that timer only while a carpet is active; never refill it on subsequent frames.
        if (Player.carpetFrame >= 0 && Player.carpetTime > hoverDuration)
            Player.carpetTime = hoverDuration;

        if (Main.dedServ || Main.netMode == NetmodeID.Server || Player.carpetFrame < 0)
            return;

        SpawnShinyTailPlatformDust();
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
        if (PuppyLeashBonusService.TryGetKnockbackMultiplier(Player, out float knockbackMultiplier))
            modifiers.Knockback *= knockbackMultiplier;

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

    public override void PostUpdateEquips()
    {
        equipmentSnapshot = PuppyEquipmentScanner.Scan(Player);
        equipmentResolution = PuppyEquipmentResolver.Resolve(equipmentSnapshot);
        ApplyEquipmentDefense();
        ApplyEquipmentKnockback();
        if (IsPuppy)
        {
            foreach (string bonusText in PuppySetBonusText.GetActiveLines(equipmentResolution, forTooltip: false))
            {
                Player.setBonus = AppendSetBonusLine(Player.setBonus, bonusText);
            }
        }
    }

    private static string AppendSetBonusLine(string existing, string line)
    {
        if (string.IsNullOrEmpty(line) || existing != null && existing.Contains(line, StringComparison.Ordinal))
            return existing;
        if (string.IsNullOrWhiteSpace(existing))
            return line;
        return existing + "\n" + line;
    }

    public override void PostUpdateMiscEffects()
    {
        PuppyEquipmentStats stats = equipmentResolution.Stats;
        Player.pickSpeed -= stats.PickSpeed;
        Player.jumpSpeedBoost += stats.JumpSpeedBoost;

        HappyIfClicker();
    }

    public override void PostUpdateRunSpeeds()
    {
        PuppyEquipmentStats stats = equipmentResolution.Stats;
        Player.moveSpeed += stats.MoveSpeed;
        Player.accRunSpeed += stats.AccRunSpeed;
        Player.maxRunSpeed += stats.MaxRunSpeed;

        // Hermes-like sprint dust triggers when accRunSpeed > maxRunSpeed in vanilla HorizontalMovement.
        // Keep them equal to suppress dust while preserving movement bonuses (functional/vanity scaling is already applied in stats).
        Player.accRunSpeed = Player.maxRunSpeed;
    }

    private void ApplyEquipmentDefense()
    {
        int defense = (int)MathF.Round(equipmentResolution.Stats.Defense, MidpointRounding.AwayFromZero);
        if (defense != 0)
            Player.statDefense += defense;
    }

    private void ApplyEquipmentKnockback()
    {
        PuppyEquipmentStats stats = equipmentResolution.Stats;
        Player.GetKnockback(DamageClass.Melee) += stats.MeleeKnockbackAdditive;
        Player.GetKnockback(DamageClass.Summon).Flat += stats.SummonKnockbackFlat;
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
