using PuppyMod.Common.Data;

namespace PuppyMod.Common.Interfaces;

public interface ILeashItem : IWithRange
{
    void AffectPuppy(Terraria.Player puppy);
    string LeashTexturePath { get; }
    float PuppyPull { get; }
    float OwnerPull { get; }
    LeashPhysicsProfile Physics => new LeashPhysicsProfile();
}
