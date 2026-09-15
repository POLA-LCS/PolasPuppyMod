using TransformAPI.Core;
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
/// puppy-specific types; everything puppy lives here. Petting a normal player only produces the
/// shared animation (sound, hearts, reach arm) because <see cref="ApplyPuppyPet"/> ignores
/// non-puppy targets.
/// </summary>
public static class PuppyPettingPlugin
{
    /// <summary>Duration of the Good Puppy buff granted by petting.</summary>
    public const int PetBuffTicks = 60;

    /// <summary>Duration of the happy tail wag after being petted, read by the dog transform.</summary>
    public const int PetWagDurationTicks = 90;

    /// <summary>Pat sound. Lives here, not in the utility library, because the asset belongs to this mod.</summary>
    public static readonly SoundPad Pats = SoundPad.LoadCategory("PuppySounds/pat", volume: 0.9f);

    public static void Load(Mod mod)
    {
        PetEvents.RegisterCanPet(mod, CanPet);
        PetEvents.RegisterOnPetStart(mod, ApplyPuppyPet);
        PetEvents.RegisterOnPetHold(mod, ApplyPuppyPet);
        PetEvents.RegisterReachAngle(mod, GetPuppyReachAngle);
        PetRegistry.RegisterPetHandItem(mod, IsClicker);
        // Zoologist petting via chat button; label is API-driven ("pet <3").
        PetRegistry.RegisterNpc(ModContent.GetInstance<PuppyMod>(), NPCID.BestiaryGirl, new PetNpcDefinition(showChatButton: true, allowWorldPet: false, buttonText: "pet <3"));
        // Players are pettable only when they satisfy the puppy set condition.
        // Allow-list: all requirements must pass (additive with veto RegisterPlayerRule).
        PetRegistry.RegisterPlayerRequirement(ModContent.GetInstance<PuppyMod>(), static p => p.GetModPlayer<PuppyPlayer>().IsPuppy);
    }

    /// <summary>
    /// Puppy targets stay pettable exactly like before, and normal players are pettable too. The
    /// puppy/normal split only changes which effects are applied, never whether petting works.
    /// </summary>
    private static bool CanPet(PetContext context) => true;

    private static bool IsClicker(Item item) => item.ModItem is BaseClickerItem;

    /// <summary>Plays the pat sound around the target; guarded so it stays silent when no assets exist.</summary>
    public static void PlayPatSound(PetTarget target)
    {
        if (Main.dedServ || !target.IsActive || Pats.Count == 0)
            return;
        SoundEngine.PlaySound(Pats.GetRandom(), target.Center);
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

    /// <summary>Transformed dogs keep the tighter pat reach angle they had before the API split.</summary>
    private static float? GetPuppyReachAngle(PetTarget target)
    {
        if (!target.IsPlayer || !target.TryGetPlayer(out Player player))
            return null;
        return player.HasTransform<DogTransformation>() ? PetService.PetAngleMorphed : (float?)null;
    }
}
