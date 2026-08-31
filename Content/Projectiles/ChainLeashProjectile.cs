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
        Projectile.WhipSettings.Segments = 21;
        Projectile.WhipSettings.RangeMultiplier = 0.75f;
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

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 origin = texture.Size() / 2f;

        void PrintSegment(int i)
        {
            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;
            float rot = diff.ToRotation() - MathHelper.PiOver2;
            Color col = Lighting.GetColor(element.ToTileCoordinates());
            Vector2 pos = element + diff * 0.5f - Main.screenPosition;
            // tiny chain link at each point :3
            Main.EntitySpriteDraw(texture, pos, null, col, rot, origin, 1f, SpriteEffects.None, 0);
        }

        PrintSegment(0);
        PrintSegment(list.Count - 2);
        for (int i = 0; i < list.Count - 2; i += 3)
        {
            PrintSegment(i);
        }
        return false;
    }
}
