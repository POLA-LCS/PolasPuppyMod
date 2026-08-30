using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Projectiles;

public class ChainLeashProjectile : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.DefaultToWhip();
        Projectile.WhipSettings.Segments = 20;
        Projectile.WhipSettings.RangeMultiplier = 1f;
    }

    private float Timer
    {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        Projectile.damage = (int)(Projectile.damage * 0.5f);
        if (Main.rand.NextFloat() < 0.33f)
            target.AddBuff(BuffID.Poisoned, 300);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        List<Vector2> list = [];
        Projectile.FillWhipControlPoints(Projectile, list);

        var sel = new List<int> { 0 };
        for (int i = 2; i < list.Count - 1; i += 2)
            sel.Add(i);
        if (sel[^1] != list.Count - 1)
            sel.Add(list.Count - 1);

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);

        for (int k = 0; k < sel.Count - 1; k++)
        {
            Vector2 a = list[sel[k]];
            Vector2 b = list[sel[k + 1]];
            Vector2 diff = b - a;
            float rot = diff.ToRotation() - MathHelper.PiOver2;
            Color col = Lighting.GetColor(a.ToTileCoordinates());
            Vector2 pos = a + diff * 0.5f - Main.screenPosition;
            Vector2 scale = new(1f, diff.Length() / texture.Height);
            // chain links on evens + tip :3
            Main.EntitySpriteDraw(texture, pos, null, col, rot, origin, scale, SpriteEffects.None, 0);
        }
        return false;
    }
}
