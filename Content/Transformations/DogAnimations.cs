using System;
using SpreadsheetSplit;

namespace PuppyMod.Content.Transformations;

/// <summary>Layout and animation ranges shared by every dog transformation sheet.</summary>
public static class DogAnimations
{
    public const int SpriteWidth = 70;
    public const int SpriteHeight = 38;

    /// <summary>Distance from the top of a sprite to the dog's feet.</summary>
    public const int SpriteBaseline = 36;

    public const string Standing = "Standing";
    public const string Jumping = "Jumping";
    public const string Falling = "Falling";
    public const string Moving = "Moving";
    public const string Bending = "Bending";
    public const string BendingCycle = "BendingCycle";
    public const string Scratching = "Scratching";
    public const string ScratchingCycle = "ScratchingCycle";

    public static readonly SpriteSheet Sheet = Create();

    private static SpriteSheet Create()
    {
        return new SpriteSheet(70, 1064, SpriteWidth, SpriteHeight)
            .WithAnimations(
                (Standing, 0..8),
                (Jumping, 8..9),
                (Falling, 8..9),
                (Moving, 9..17),
                (Bending, 17..23),
                (BendingCycle, 18..22),
                (Scratching, 23..28),
                (ScratchingCycle, 24..27));
    }
}
