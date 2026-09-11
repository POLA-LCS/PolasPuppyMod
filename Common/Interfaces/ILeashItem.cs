using PuppyMod.Common.Physics;
using Terraria;

namespace PuppyMod.Common.Interfaces;

public interface ILeashItem : IWithRange
{
    void AffectPuppy(Player puppy);

    string LeashTexturePath { get; }

    LeashPhysicsProfile Physics => new();
}
