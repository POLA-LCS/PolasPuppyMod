namespace PuppyMod.Common.Interfaces;

public interface ILeashItem : IWithRange
{
    void AffectPuppy(Terraria.Player puppy);
    string LeashTexturePath { get; }
    float PuppyPull { get; }
    float OwnerPull { get; }
}
