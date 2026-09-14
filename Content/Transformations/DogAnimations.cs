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
        SpriteSheet sheet = new(70, 1064, SpriteWidth, SpriteHeight);

        sheet.Animations[Standing] = 0..8;
        sheet.Animations[Jumping] = 8..9;
        sheet.Animations[Falling] = 8..9;
        sheet.Animations[Moving] = 9..17;
        sheet.Animations[Bending] = 17..23;
        sheet.Animations[BendingCycle] = 18..22;
        sheet.Animations[Scratching] = 23..28;
        sheet.Animations[ScratchingCycle] = 24..27;

        return sheet;
    }
}
