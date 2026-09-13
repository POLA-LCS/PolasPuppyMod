using PuppyMod.Players;
using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.GlobalItems
{
    public class DogTailGlobalItem : PuppyGlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogTail;

        public override string EarOrTailName => "Tail";
        public override bool HasAccessory => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PuppyPlayer>().EquipmentSnapshot.ContainsFunctional(ItemID.DogTail);
        public override bool HasVanity => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PuppyPlayer>().EquipmentSnapshot.ContainsVanity(ItemID.DogTail);
        public override string AccessoryTypeString => "Increase mobility";
        public override string FlavorText => "'Walkies = zoomies! Aarr-woof! :3'";
    }
}
