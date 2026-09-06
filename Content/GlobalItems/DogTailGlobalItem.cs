using PuppyMod.Players;
using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.GlobalItems
{
    public class DogTailGlobalItem : PuppyGlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogTail;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            puppy.HasDogTailAccessory = true;
        }

        public override void UpdateVanity(Item item, Player player)
        {
            var puppy = player.GetModPlayer<PuppyPlayer>();
            puppy.HasDogTailVanity = true;
        }

        public override string EarOrTailName => "Tail";
        public override bool HasAccessory => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PolasBasePlayer>().HasInAccessory(ItemID.DogTail);
        public override bool HasVanity => Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.GetModPlayer<PolasBasePlayer>().HasInVanity(ItemID.DogTail);
        public override string AccessoryTypeString => "Increase mobility";
        public override string FlavorText => "'Walkies = zoomies! Aarr-woof! :3'";
    }
}
