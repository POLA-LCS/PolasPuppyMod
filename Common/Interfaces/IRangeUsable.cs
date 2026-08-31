namespace PuppyMod.Common.Interfaces;

public interface IRangeUsable
{
    int RangeTiles { get; }
    float RangePixels => RangeTiles * 16f;
}
