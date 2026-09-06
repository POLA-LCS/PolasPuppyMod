using PuppyMod.Players;
using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.GlobalItems
{
    public class DogEarsGlobalItem : PuppyGlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogEars;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            puppy.HasDogEarsAccessory = true;
        }

        public override void UpdateVanity(Item item, Player player)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            puppy.HasDogEarsVanity = true;
        }

        public override string EarOrTailName => "Ears";
        public override bool HasAccessory => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PolasBasePlayer>().HasInAccessory(ItemID.DogEars);
        public override bool HasVanity => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PolasBasePlayer>().HasInVanity(ItemID.DogEars);
        public override string AccessoryTypeString => "Increase digging speed";
        public override string FlavorText => "'Let's make a hole! *paw paw* :3'";
    }
}
