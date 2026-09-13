using Terraria;
using Terraria.ModLoader;
using PuppyMod.Common.Utils;

namespace PuppyMod.Content.Buffs;

public class GoodPuppyBuff : ModBuff
{
    public override string Texture => AssetUtils.GetBuffTexturePathWithFallback(nameof(GoodPuppyBuff));

    /// <summary>Life regen bonus (14) – substantial heal for Good Puppy buff; kept as named const for audit.</summary>
    public const int LifeRegen = 14;
    /// <summary>Movement bonus: +0.6 moveSpeed (~60% faster).</summary>
    public const float MoveSpeed = 0.6f;
    /// <summary>Acceleration run bonus 1.5 – matches max to suppress Hermes dust.</summary>
    public const float AccRunSpeed = 1.5f;
    /// <summary>Max run speed bonus 1.5 – equal to AccRunSpeed to avoid dust trigger.</summary>
    public const float MaxRunSpeed = 1.5f;
    /// <summary>Jump boost +1.0 – extra jump height while buff active.</summary>
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