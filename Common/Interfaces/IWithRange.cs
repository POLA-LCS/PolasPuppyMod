using PuppyMod.Common.Utils;

namespace PuppyMod.Common.Interfaces;

public interface IWithRange
{
    int RangeTiles { get; }

    /// <summary>Range in pixels (tiles * 16px).</summary>
    float RangePixels => RangeTiles * DistanceUtils.TilePixels;
}
