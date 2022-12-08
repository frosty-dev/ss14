using Robust.Shared.Map;

namespace Content.Shared.White.Line;

public interface ILine
{
    bool HasSegments();
}

public interface ITimedLine : ILine
{
    void AddLifetime(float time);
    void ResetLifetime();
    void RemoveExpiredSegments();
}

public interface IDynamicLine<TSettings> : ILine
{
    MapId MapId { get; set; }
    TSettings Settings { get; set; }
    void TryCreateSegment(MapCoordinates coords);
    void UpdateSegments(float dt);
}

public interface IDrawableLine<TDrawData> : ILine
{
    void RecalculateDrawData();
    IEnumerable<TDrawData> GetDrawData();
}

public interface IAttachedLine : ILine
{
    bool Attached { get; set; }
}

