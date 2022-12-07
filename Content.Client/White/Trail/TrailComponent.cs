using Content.Shared.White.Trail;
using Robust.Shared.Map;
using System.Linq;

namespace Content.Client.White.Trail;

[RegisterComponent]
public sealed class TrailComponent : SharedTrailComponent
{
    [ViewVariables]
    public TrailData? Data { get; set; } = null;
    [ViewVariables]
    public override TrailSettings Settings { get; set; } = TrailSettings.Default;
}

[DataDefinition]
public sealed class TrailData
{

    [ViewVariables]
    public TrailSettings Settings { get; set; } = default!;
    [ViewVariables]
    public LinkedList<TrailSegment> Segments { get; } = new();
    [ViewVariables]
    public float LifetimeAccumulator { get; set; } //не доживет до ошибок с плавающей точкой надеюсь
    [ViewVariables]
    public MapCoordinates? LastParentCoords { get; set; } = null;
    [ViewVariables]
    public IEnumerable<TrailSegmentDrawData> CalculatedDrawData { get; set; } = Enumerable.Empty<TrailSegmentDrawData>();

    public TrailData(TrailSettings settings)
    {
        Settings = settings;
    }
}

public struct TrailSegmentDrawData
{
    public readonly Vector2 Point1;
    public readonly Vector2 Point2;
    public readonly Angle AngleRight;
    public readonly float LifetimePercent;
    public readonly TrailSegment? Segment;

    public TrailSegmentDrawData(Vector2 point1, Vector2 point2, Angle angleLeft, float lifetimePercent, TrailSegment? segment = null)
    {
        Point1 = point1;
        Point2 = point2;
        AngleRight = angleLeft;
        LifetimePercent = lifetimePercent;
        Segment = segment;
    }
}

public sealed class TrailSegment
{
    public TrailSegment(MapCoordinates coords, float existTil)
    {
        Coords = coords;
        ExistTil = existTil;
    }

    [ViewVariables]
    public MapCoordinates Coords { get; set; }
    [ViewVariables]
    public float ExistTil { get; set; }
}
