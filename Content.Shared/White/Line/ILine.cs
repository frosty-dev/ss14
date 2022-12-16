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
    void TryCreateSegment((Vector2 WorldPosition, Angle WorldRotation) worldPosRot, MapId mapId);
    void UpdateSegments(float dt);
}

public interface IAttachedLine : ILine
{
    bool Attached { get; set; }
}

