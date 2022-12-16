using Content.Shared.White.Line;
using Content.Shared.White.Trail;
using Robust.Shared.Map;

namespace Content.Client.White.Line.Manager;

public interface ITrailLineManager<out TTrailLine> : IDynamicLineManager<TTrailLine, ITrailSettings>
    where TTrailLine : ITrailLine
{
    TTrailLine CreateTrail(ITrailSettings settings, MapId mapId);
}
