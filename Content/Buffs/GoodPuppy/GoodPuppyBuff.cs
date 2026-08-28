using Terraria;
using Terraria.ModLoader;

namespace PuppyMod.Content.Buffs.GoodPuppy;

public class GoodPuppyBuff : ModBuff
{

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        // Get buffed you good little puppy, you deserve it
        player.lifeRegen += 14;
        player.moveSpeed += 0.6f;
        player.accRunSpeed += 1.5f;
        player.jumpSpeedBoost += 1.0f;
    }
}
