namespace CourseWork.MapGen.Helpers;

public sealed class TilesHeat : TilesProperty
{
    public Constants.HeatType THeat { get; }

    public TilesHeat(Constants.HeatType heat, RgbColor color) : base(color)
    {
        THeat = heat;
    }
}