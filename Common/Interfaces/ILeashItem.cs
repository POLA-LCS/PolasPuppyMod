using Microsoft.Xna.Framework;

namespace PuppyMod.Common.Interfaces;

public interface ILeashItem : IRangeUsable
{
    void AffectPuppy(Terraria.Player puppy);
    string RopeTexturePath { get; }
    Color RopeColor { get; }
    float PuppyPull { get; }
    float OwnerPull { get; }
}
