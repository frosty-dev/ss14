using Content.Client.White.Trail.SplineRenderer;
using Content.Shared.White.Spline;
using Content.Shared.White.Trail;
using Robust.Shared.Map;

namespace Content.Client.White.Trail.Line.Manager;

public sealed class TrailSplineManager : ITrailLineManager
{
    private readonly LinkedList<TrailSpline> _lines = new();
    public IEnumerable<ITrailLine> Lines => _lines;
    public ITrailLine CreateTrail(ITrailSettings settings, MapId mapId)
    {
        var tline = new TrailSpline
        {
            Attached = true,
            Settings = settings,
            MapId = mapId,
            SplineIterator = Spline.From2DType(settings.SplineIteratorType),
            GradientIterator = Spline.From4DType(settings.GradientIteratorType),
            Renderer = TrailSplineRenderer.FromType(settings.SplineRendererType)
        };

        _lines.AddLast(tline);
        return tline;
    }

    public void Detach(ITrailLineHolder holder)
    {
        if (holder.TrailLine is TrailSpline trailSpline)
        {
            trailSpline.Attached = false;
            var detachedSettings = new TrailSettings();
            TrailSettings.Inject(detachedSettings, trailSpline.Settings);
            trailSpline.Settings = detachedSettings;
        }
    }

    public void Update(float dt)
    {
        var curNode = _lines.First;
        while (curNode != null)
        {
            var curLine = curNode.Value;
            curNode = curNode.Next;

            if (!curLine.HasSegments())
            {
                if (curLine.Attached)
                    curLine.ResetLifetime();
                else
                    _lines.Remove(curLine);
                continue;
            }

            curLine.AddLifetime(dt);
            curLine.RemoveExpiredSegments();
            curLine.UpdateSegments(dt);
        }
    }
}

