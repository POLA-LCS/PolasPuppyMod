namespace PuppyMod.Common.Interfaces;

public interface IWithRange
{
    int RangeTiles { get; }
    float RangePixels => RangeTiles * 16f;
}
