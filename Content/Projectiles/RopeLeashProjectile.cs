using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace PuppyMod.Content.Projectiles;
public class RopeLeashProjectile : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.IsAWhip[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.DefaultToWhip();
        Projectile.WhipSettings.Segments = 17;
        Projectile.WhipSettings.RangeMultiplier = 0.50f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        Projectile.damage = (int)(Projectile.damage * 0.75f);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        List<Vector2> controlPoints = [];
        Projectile.FillWhipControlPoints(Projectile, controlPoints);

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 origin = texture.Size() / 2f;

        void DrawSegment(int i)
        {
            Vector2 element = controlPoints[i];
            Vector2 diff = controlPoints[i + 1] - element;
            float rot = diff.ToRotation() - MathHelper.PiOver2;
            Color col = Lighting.GetColor(element.ToTileCoordinates());
            Vector2 pos = element + diff * 0.5f - Main.screenPosition;
            Main.EntitySpriteDraw(texture, pos, null, col, rot, origin, 1f, SpriteEffects.None, 0);
        }

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            DrawSegment(i);
        }

        return false;
    }
}
