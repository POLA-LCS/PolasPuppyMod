using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Common.Interfaces;
using PuppyMod.Common.PuppySets;

namespace PuppyMod.Content.Items.Tail;

public class ReinforcedTailItem : ModItem, IPuppyTail
{
    public PuppyEquipmentStats Stats => new(
        Defense: 2f,
        MoveSpeed: 0.20f,
        AccRunSpeed: 0.30f,
        MaxRunSpeed: 0.20f,
        JumpSpeedBoost: 0.6666667f);

    public override string Texture => "Terraria/Images/Item_" + ItemID.DogTail;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DogTail);
    }
}
