using Content.Shared.White.Trail;
using Robust.Client.Graphics;

namespace Content.Shared.White.Line;

public interface IRenderableLine : ILine
{
    void Render(DrawingHandleWorld handle, Texture? texture);
}

public interface ITrailLine : ITimedLine, IAttachedLine, IDynamicLine<ITrailSettings>, IRenderableLine { }
