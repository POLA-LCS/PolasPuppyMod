using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.GlobalItems
{
    // H7: Dual GlobalItems intentional – separate filter per item type (DogTail) alongside DogEarsGlobalItem.
    // Keeps AppliesToEntity narrow for tModLoader caching; merging into single PuppyGlobalItem with type==DogEars||DogTail is alternative.
    public class DogTailGlobalItem : PuppyGlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogTail;
    }
}
