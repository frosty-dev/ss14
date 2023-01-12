using Content.Shared.White.Trail;
using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Client.White.Trail.Line;

public interface ITrailLine
{
    ITrailSettings Settings { get; }
    void TryCreateSegment((Vector2 WorldPosition, Angle WorldRotation) worldPosRot, MapId mapId);
    void Render(DrawingHandleWorld handle, Texture? texture);
}
