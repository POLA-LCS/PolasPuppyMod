using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PetAnyone;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace PuppyMod.Services.Petting;

/// <summary>
/// Chat-button glue for NPC petting. Hooks the private static
/// Terraria.Main.DrawNPCChatButtons via MonoMod (TerrariaHooks) and draws an
/// extra "pet <3" (or per-NPC custom) button after all vanilla buttons.
/// Position is rebuilt from the local IL so we do not pull external sources.
///
/// Layout (from IL):
///   float y = 130 + numLines*30f;
///   float x = 180 + (Main.screenWidth-800)/2f;
///   Adv(string t) => (size.X capped at 260 +30);
///   x += Adv(focusText); x += Adv(Lang.inter[52].Value); x += Adv(focusText3); x += Adv(Happiness) if present;
///   our button at (x,y) with label = def.ButtonText.
/// </summary>
internal sealed class PetNpcChatButtonSystem : ModSystem
{
    private static bool _hovered;

    public override void Load()
    {
        On_Main.DrawNPCChatButtons += DrawNPCChatButtonsHook;
    }

    public override void Unload()
    {
        On_Main.DrawNPCChatButtons -= DrawNPCChatButtonsHook;
        _hovered = false;
    }

    private static void DrawNPCChatButtonsHook(On_Main.orig_DrawNPCChatButtons orig, int superColor, Color chatColor, int numLines, string focusText, string focusText3)
    {
        orig(superColor, chatColor, numLines, focusText, focusText3);

        if (Main.dedServ)
            return;

        Player local = Main.LocalPlayer;
        if (local == null || !local.active)
            return;

        int talk = local.talkNPC;
        if (talk < 0 || talk >= Main.npc.Length)
            return;
        NPC npc = Main.npc[talk];
        if (npc == null || !npc.active)
            return;

        if (!PetRegistry.TryGetNpcDefinition(npc, out var def) || def == null || !def.ShowChatButton)
            return;

        // Rebuild vanilla layout exactly from local IL.
        float y = 130f + numLines * 30f;
        float x = 180f + (Main.screenWidth - 800) / 2f;
        var font = FontAssets.MouseText.Value;
        Vector2 scale09 = new(0.9f);

        float Adv(string t)
        {
            if (string.IsNullOrEmpty(t))
                return 0f;
            Vector2 sz = ChatManager.GetStringSize(font, t, scale09, -1f);
            float w = sz.X;
            if (w > 260f)
                w = 260f;
            return w + 30f;
        }

        if (!string.IsNullOrEmpty(focusText))
            x += Adv(focusText);
        // Close button is always present - Lang.inter[52] == "Close"
        x += Adv(Lang.inter[52].Value);
        if (!string.IsNullOrWhiteSpace(focusText3))
            x += Adv(focusText3);
        // Happiness report button
        if (!Main.remixWorld && Main.LocalPlayer.currentShoppingSettings.HappinessReport != "")
            x += Adv(Language.GetTextValue("UI.NPCCheckHappiness"));

        Vector2 pos = new(x, y);
        string label = def.ButtonText; // already fallback to "pet <3"
        Vector2 size = ChatManager.GetStringSize(font, label, scale09, -1f);
        Vector2 grow = Vector2.One;
        if (size.X > 260f)
            grow.X = 260f / size.X;

        Vector2 mouse = new(Main.mouseX, Main.mouseY);
        // hover rect = pos .. pos + size*scale09*grow
        Vector2 hoverSize = size * scale09 * grow;
        // grow is Vec( X scale, 1 ) so hoverSize.Y = size.Y*0.9*1, X = size.X*0.9*grow.X
        bool hovered = Utils.Between(mouse, pos, pos + hoverSize);

        bool wasHovered = _hovered;
        // We'll need a mutable scale for hover enlarge; keep base scale09 unchanged for size calc above
        Vector2 drawScale = scale09 * grow;
        Color shadow = Color.Black;
        // drawScale will be enlarged on hover
        if (hovered && !PlayerInput.IgnoreMouseInterface)
        {
            local.mouseInterface = true;
            local.releaseUseItem = false;
            drawScale *= 1.2f;
            if (!wasHovered)
                SoundEngine.PlaySound(SoundID.MenuTick);
            _hovered = true;
            shadow = Color.Brown;
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                Main.mouseLeftRelease = false;
                TryPetNpc(npc);
            }
        }
        else
        {
            if (wasHovered)
                SoundEngine.PlaySound(SoundID.MenuClose);
            _hovered = false;
            shadow = Color.Black;
        }

        // Draw at centered origin like vanilla (pos + size*grow*0.5 with origin size*0.5)
        SpriteBatch sb = Main.spriteBatch;
        Vector2 origin = size * 0.5f;
        Vector2 drawPos = pos + size * grow * 0.5f;
        // Reuse hover enlarged drawScale if hovered, else base
        // drawScale already reflects hover 1.2x
        ChatManager.DrawColorCodedStringWithShadow(sb, font, label, drawPos, chatColor, shadow, 0f, origin, drawScale, -1f, 2f);
    }

    private static void TryPetNpc(NPC npc)
    {
        if (Main.dedServ)
            return;
        Player local = Main.LocalPlayer;
        if (local == null || !local.active)
            return;
        if (local.whoAmI != Main.myPlayer)
            return;
        if (local.talkNPC < 0)
            return;
        if (npc == null || !npc.active)
            return;
        if (!PetRegistry.IsNpcPettable(npc))
            return;

        PetTarget target = PetTarget.FromNpc(npc);
        if (!PetService.CanPet(local, target))
            return;

        // Local prediction: apply core, hearts, sound, reach.
        bool applied = PetService.ApplyPetCore(local, target, bypassCooldown: false);
        if (!applied)
            return;

        // Spawn heart respecting interval (HandleSynced also does, but we want immediate heart)
        PetService.PlayPetHeartSynced(target);
        PuppyPettingPlugin.PlayPatSound(target);
        local.GetModPlayer<PettingPlayer>().SetReach(target);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModContent.GetInstance<PuppyMod>().RequestPetNpc(npc.whoAmI);
        }
        // SinglePlayer already handled; Server not reached via UI.
    }
}
