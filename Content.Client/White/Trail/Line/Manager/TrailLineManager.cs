using Content.Client.White.Line.Manager;
using Content.Shared.White.Line;
using Content.Shared.White.Trail;
using Robust.Shared.Map;
using System.Runtime.CompilerServices;

namespace Content.Client.White.Trail.Line.Manager;

public sealed class TrailLineManager : ITrailLineManager<ITrailLine>
{
    private readonly LinkedList<ITrailLine> _lines = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<ITrailLine> GetLines() => _lines;

    public ITrailLine CreateTrail(ITrailSettings settings, MapId mapId)
    {
        var tline = CreateTrailByType(settings.CreatedTrailType);
        tline.Attached = true;
        tline.Settings = settings;
        tline.MapId = mapId;

        _lines.AddLast(tline);
        return tline;
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

    private static ITrailLine CreateTrailByType(TrailLineType trailType)
        => trailType switch
        {
            TrailLineType.ContiniousStretch => new TrailLineContiniousStretch(),
            TrailLineType.PointCatmullRom => new TrailLinePointCatmullRom(),
            _ => throw new NotImplementedException()
        };
}

