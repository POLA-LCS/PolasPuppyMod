using Terraria;
using Terraria.ModLoader;

namespace PuppyMod.Content.Buffs.GoodPuppy;

public class GoodPuppyBuff : ModBuff
{
    public const int LifeRegen = 14;
    public const float MoveSpeed = 0.6f;
    public const float AccRunSpeed = 1.5f;
    public const float JumpBoost = 1.0f;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // good little puppy :3
        player.lifeRegen += LifeRegen;
        player.moveSpeed += MoveSpeed;
        player.accRunSpeed += AccRunSpeed;
        player.jumpSpeedBoost += JumpBoost;
    }
}
