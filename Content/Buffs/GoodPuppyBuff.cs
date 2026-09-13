using Terraria;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;

namespace PuppyMod.Content.Buffs;

public class GoodPuppyBuff : ModBuff
{
    public override string Texture => AssetUtils.GetBuffTexturePathWithFallback(nameof(GoodPuppyBuff));

    /// <summary>Life regen bonus.</summary>
    public const int LifeRegen = 14;
    /// <summary>Movement speed bonus.</summary>
    public const float MoveSpeed = 0.6f;
    /// <summary>Acceleration run bonus.</summary>
    public const float AccRunSpeed = 1.5f;
    /// <summary>Max run speed bonus.</summary>
    public const float MaxRunSpeed = 1.5f;
    /// <summary>Jump speed bonus.</summary>
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
        // Keep acceleration equal to max run speed to avoid sprint dust.
        if (player.accRunSpeed > player.maxRunSpeed)
            player.accRunSpeed = player.maxRunSpeed;
    }
}