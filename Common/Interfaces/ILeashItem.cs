namespace PuppyMod.Common.Interfaces;

public interface ILeashItem : IRangeUsable
{
    void AffectPuppy(Terraria.Player puppy);
    string RopeTexturePath { get; }
    float PuppyPull { get; }
    float OwnerPull { get; }
}
