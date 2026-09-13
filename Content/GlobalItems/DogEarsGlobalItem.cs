using Terraria;
using Terraria.ID;

namespace PuppyMod.Content.GlobalItems;

public class DogEarsGlobalItem : PuppyGlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.DogEars;
}
