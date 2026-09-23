using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TransLib.Core;
using PetAnyone;
using PuppyMod.Common.Utils;
using PuppyMod.Content.Buffs;
using PuppyMod.Content.Items.Clickers;
using PuppyMod.Content.Transformations;
using PuppyMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Services.Petting;

/// <summary>
/// Hooks PuppyMod's own behavior onto the generic petting API. The API itself stays free of
/// puppy-specific types and visuals; everything puppy lives here, including the heart particles
/// shown while a target is being petted.
/// </summary>
public static class PuppyPettingPlugin
{
    /// <summary>Duration of the Good Puppy buff granted by petting.</summary>
    public const int PetBuffTicks = 60;

    /// <summary>Duration of the happy tail wag after being petted, read by the dog transform.</summary>
    public const int PetWagDurationTicks = 90;

    /// <summary>Minimum ticks between heart particles per target while petting.</summary>
    public const int PetHeartIntervalTicks = 30;

    /// <summary>Gore type of the full bright heart, the Love Potion visual.</summary>
    private const int HeartGoreType = 331;

    /// <summary>Pat sound. Lives here, not in the utility library, because the asset belongs to this mod.</summary>
    public static readonly SoundPad Pats = SoundPad.LoadCategory("PuppySounds/pat", volume: 0.9f);

    private static readonly Dictionary<(PetTargetKind Kind, int Index), long> LastHeartTick = new();

    public static void Load(Mod mod)
    {
        PetEvents.OnPetStart(mod, OnPetStart);
        PetEvents.OnPetHold(mod, OnPetHold);
        PetEvents.OnPetEnd(mod, OnPetEnd);
        PetEvents.OnReachAngle(mod, GetPuppyReachAngle);
        PetRegistry.RegisterPetHandItem(mod, IsClicker);
        // Zoologist petting via chat button; label is API-driven ("Pet <3").
        PetRegistry.RegisterNpc(ModContent.GetInstance<PuppyMod>(), NPCID.BestiaryGirl, new PetNpcDefinition(showChatButton: true, allowWorldPet: false, buttonText: "Pet <3"));
        // Players are pettable only when they satisfy the puppy set condition.
        // Allow-list: all requirements must pass (additive with veto RegisterPlayerRule).
        PetRegistry.RegisterPlayerRequirement(ModContent.GetInstance<PuppyMod>(), static p => p.GetModPlayer<PuppyPlayer>().IsPuppy);
    }

    private static bool IsClicker(Item item) => item.ModItem is BaseClickerItem;

    /// <summary>Plays the pat sound around the target; guarded so it stays silent when no assets exist.</summary>
    public static void PlayPatSound(PetTarget target)
    {
        if (Main.dedServ || !target.IsActive || Pats.Count == 0)
            return;
        SoundEngine.PlaySound(Pats.GetRandom(), target.Center);
    }

    private static void OnPetStart(PetStartEvent e)
    {
        ApplyPuppyPet(e.Context);
        SpawnHeartIfDue(e.Target);
    }

    private static void OnPetHold(PetHoldEvent e)
    {
        ApplyPuppyPet(e.Context);
        SpawnHeartIfDue(e.Target);
    }

    private static void OnPetEnd(PetEndEvent e)
    {
        LastHeartTick.Remove((e.Target.Kind, e.Target.Index));
    }

    private static void ApplyPuppyPet(PetContext context)
    {
        if (!context.Target.IsPlayer || !context.Target.TryGetPlayer(out Player target))
            return;

        PuppyPlayer puppy = target.GetModPlayer<PuppyPlayer>();
        if (!puppy.IsPuppy)
            return;

        puppy.PatWagTicks = PetWagDurationTicks;
        target.AddBuff(ModContent.BuffType<GoodPuppyBuff>(), PetBuffTicks);
    }

    /// <summary>
    /// Spawns one heart above the target at most once per <see cref="PetHeartIntervalTicks"/>.
    /// The API raises the hooks, this mod owns the visual.
    /// </summary>
    private static void SpawnHeartIfDue(PetTarget target)
    {
        if (Main.dedServ || !target.IsActive)
            return;

        long now = Main.GameUpdateCount;
        var key = (target.Kind, target.Index);
        if (LastHeartTick.TryGetValue(key, out long last) && now - last < PetHeartIntervalTicks)
            return;

        LastHeartTick[key] = now;

        if (!target.TryGetEntity(out Entity entity) || entity is null)
            return;

        Vector2 position = entity.Center
            + new Vector2(0f, -entity.Hitbox.Height * 0.6f)
            + Main.rand.NextVector2Circular(6f, 4f);

        int index = Gore.NewGore(
            entity.GetSource_FromThis(),
            position,
            Vector2.Zero,
            HeartGoreType,
            Main.rand.NextFloat(0.55f, 0.85f));

        if (index < 0 || index >= Main.gore.Length)
            return;

        Gore heart = Main.gore[index];
        heart.sticky = false;
        heart.velocity = new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), Main.rand.NextFloat(-0.9f, -0.5f));
        heart.rotation = 0f;
        heart.timeLeft = 70;
    }

    /// <summary>Transformed dogs keep the tighter pat reach angle they had before the API split.</summary>
    private static float? GetPuppyReachAngle(PetTarget target)
    {
        if (!target.IsPlayer || !target.TryGetPlayer(out Player player))
            return null;
        return player.HasTransform<DogTransformation>() ? PetService.PetAngleMorphed : (float?)null;
    }
}
