using PuppyMod.Players;
using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.GlobalItems
{
    public class DogEarsGlobalItem : PuppyGlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogEars;

        public override string EarOrTailName => "Ears";
        public override bool HasAccessory => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PuppyPlayer>().EquipmentSnapshot.ContainsFunctional(ItemID.DogEars);
        public override bool HasVanity => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PuppyPlayer>().EquipmentSnapshot.ContainsVanity(ItemID.DogEars);
        public override string AccessoryTypeString => "Increase digging speed";
        public override string FlavorText => "'Let's make a hole! *paw paw* :3'";
    }
}
