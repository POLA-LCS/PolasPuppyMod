using Microsoft.Xna.Framework;
using PetAnyone;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Services.Petting;

/// <summary>
/// tModLoader glue for the generic petting API: owns the right-click hold loop, the per-player
/// reach animation state and every netmode branch. The shared rules and visuals live in the
/// PetAnyone utility library; puppy-specific effects are applied by <see cref="PuppyPettingPlugin"/>.
/// </summary>
public class PettingPlayer : ModPlayer
{
    /// <summary>Local tick of the last pet attempt, so holding right-click doesn't spam taps.</summary>
    public int PatterPetTick = -PetService.PetCooldownTicks;

    /// <summary>Ticks left of the reach arm animation.</summary>
    public int PetReachTicks;

    /// <summary>Target the reach arm is reaching toward, or <see cref="PetTarget.None"/>.</summary>
    public PetTarget PetReachTarget = PetTarget.None;

    /// <summary>Timer that spaces hold refreshes to <see cref="PetService.PetRefreshTicks"/>.</summary>
    public int PetRefreshTimer;

    private PetTarget _activePetTarget = PetTarget.None;

    public override void PostUpdate()
    {
        UpdateReach();

        if (Player.whoAmI == Main.myPlayer)
            HandlePetInput();
    }

    /// <summary>
    /// Applies a received pet broadcast on this client: mod sound, shared hearts/events and the
    /// patter's reach animation. Player targets can be null if the sender is unknown.
    /// </summary>
    public static void ApplySyncedPet(Player patter, PetTarget target)
    {
        if (!target.IsActive)
            return;

        PuppyPettingPlugin.PlayPatSound(target);
        PetService.HandleSyncedPet(patter, target);

        if (patter != null && patter.active && !(target.IsPlayer && patter.whoAmI == target.Index))
            patter.GetModPlayer<PettingPlayer>().SetReach(target);
    }

    /// <summary>Starts the reach animation toward the target and turns the player toward it.</summary>
    public void SetReach(PetTarget target)
    {
        if (!target.IsActive)
            return;

        PetReachTicks = PetService.PetReachDurationTicks;
        PetReachTarget = target;
        Player.direction = target.Center.X >= Player.Center.X ? 1 : -1;
    }

    private void HandlePetInput()
    {
        bool wantsToPet = Player.controlUseTile && PetService.IsPetHand(Player);
        if (!wantsToPet || !PetService.TryFindTargetUnderCursor(Player, out PetTarget target) || !PetService.CanPet(Player, target))
        {
            PetRefreshTimer = 0;
            EndActivePet();
            return;
        }

        // NPC world petting requires the registration to opt in.
        if (target.IsNpc)
        {
            if (!target.TryGetNpc(out NPC n) || !PetRegistry.TryGetNpcDefinition(n, out var def) || def == null || !def.AllowWorldPet)
            {
                PetRefreshTimer = 0;
                EndActivePet();
                return;
            }
        }

        if (_activePetTarget != target)
        {
            EndActivePet();
            _activePetTarget = target;
        }

        // The initial tap respects the 20 tick cooldown through TryPet; the hold refresh bypasses it.
        int tickBefore = PatterPetTick;
        TryPet(target);
        if (PatterPetTick != tickBefore)
            PetRefreshTimer = 0; // avoid a double packet on the same tick as the tap

        PetRefreshTimer++;
        if (PetRefreshTimer >= PetService.PetRefreshTicks)
        {
            PetRefreshTimer = 0;
            HoldPet(target);
        }

        // The service throttles to one heart per PetHeartIntervalTicks and target.
        if (PetService.CanPet(Player, target))
            PetService.PlayPetHeartSynced(target);
    }

    private bool TryPet(PetTarget target)
    {
        if (!PetService.CanPet(Player, target))
            return false;

        int now = (int)Main.GameUpdateCount;
        if (now - PatterPetTick < PetService.PetCooldownTicks)
            return false;

        PatterPetTick = now;
        SetReach(target);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // NPC petting over the network arrives in a later phase; player petting uses the
            // existing Pat packet, whose layout stays untouched.
            if (target.IsPlayer)
                ModContent.GetInstance<PuppyMod>().RequestPet(target.Index);
            else if (target.IsNpc)
                ModContent.GetInstance<PuppyMod>().RequestPetNpc(target.Index);
            return true;
        }

        if (Main.netMode == NetmodeID.Server)
        {
            if (PetService.ApplyPetCore(Player, target, bypassCooldown: false))
            {
                if (target.IsPlayer)
                    ModContent.GetInstance<PuppyMod>().BroadcastPet(Player.whoAmI, target.Index);
                else if (target.IsNpc)
                    ModContent.GetInstance<PuppyMod>().BroadcastPetNpc(Player.whoAmI, target.Index);
            }
            return true;
        }

        if (PetService.ApplyPetCore(Player, target, bypassCooldown: false))
            PuppyPettingPlugin.PlayPatSound(target);
        return true;
    }

    private bool HoldPet(PetTarget target)
    {
        if (!PetService.CanPet(Player, target))
            return false;

        SetReach(target);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            if (target.IsPlayer)
                ModContent.GetInstance<PuppyMod>().RequestPet(target.Index);
            else if (target.IsNpc)
                ModContent.GetInstance<PuppyMod>().RequestPetNpc(target.Index);
            // Local prediction for responsiveness; the server broadcast stays authoritative.
            PetService.ApplyPetCore(Player, target, bypassCooldown: true);
            return true;
        }

        if (Main.netMode == NetmodeID.Server)
        {
            if (PetService.ApplyPetCore(Player, target, bypassCooldown: true))
            {
                if (target.IsPlayer)
                    ModContent.GetInstance<PuppyMod>().BroadcastPet(Player.whoAmI, target.Index);
                else if (target.IsNpc)
                    ModContent.GetInstance<PuppyMod>().BroadcastPetNpc(Player.whoAmI, target.Index);
            }
            return true;
        }

        PetService.ApplyPetCore(Player, target, bypassCooldown: true);
        return true;
    }

    private void EndActivePet()
    {
        if (_activePetTarget == PetTarget.None)
            return;

        PetEvents.RaisePetEnd(new PetContext(Player, _activePetTarget));
        _activePetTarget = PetTarget.None;
    }

    private void UpdateReach()
    {
        if (PetReachTicks <= 0)
        {
            PetReachTarget = PetTarget.None;
            return;
        }

        PetReachTicks--;
        if (!PetReachTarget.IsActive)
            return;

        // Same bobbing pose vanilla uses when petting town pets.
        int stretch = (Main.GameUpdateCount % 14) / 7 == 1 ? 0 : 3;
        float angle = PetService.GetReachAngle(PetReachTarget);
        Player.SetCompositeArmBack(true, (Player.CompositeArmStretchAmount)stretch, angle * -MathHelper.TwoPi * Player.direction);
    }
}
