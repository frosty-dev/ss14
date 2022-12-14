﻿using Content.Shared.White.Line;
using Content.Shared.White.Trail;

namespace Content.Client.White.Line.Manager;

public interface ITrailLineManager<out TTrailLine> : IDynamicLineManager<TTrailLine, ITrailSettings> where TTrailLine : ITrailLine { }
