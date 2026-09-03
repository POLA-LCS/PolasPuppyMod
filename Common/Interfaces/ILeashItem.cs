using PuppyMod.Common.Data;

namespace PuppyMod.Common.Interfaces;

public interface ILeashItem : IWithRange
{
    void AffectPuppy(Terraria.Player puppy);
    string LeashTexturePath { get; }
    LeashPhysicsProfile Physics => new();
}
