using Terraria;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;

namespace PuppyMod.Content.Buffs.GoodPuppy;

public class GoodPuppyBuff : ModBuff
{
    public override string Texture => AssetUtils.GetBuffTexturePathWithFallback(nameof(GoodPuppyBuff));

    public const int LifeRegen = 14;
    public const float MoveSpeed = 0.6f;
    public const float AccRunSpeed = 1.5f;
    public const float MaxRunSpeed = 1.5f;
    public const float JumpBoost = 1.0f;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.lifeRegen += LifeRegen;
        player.moveSpeed += MoveSpeed;
        player.accRunSpeed += AccRunSpeed;
        player.maxRunSpeed += MaxRunSpeed;
        player.jumpSpeedBoost += JumpBoost;
        // Keep acc == max to suppress Hermes sprint dust (acc > max triggers HorizontalMovement dust).
        if (player.accRunSpeed > player.maxRunSpeed)
            player.accRunSpeed = player.maxRunSpeed;
    }
}