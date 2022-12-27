﻿using System.Net;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaSocketWrapper
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private UtkaSocket? _utkaSocket;

    private bool _initialized;
    private string _key = string.Empty;

    public void Initialize()
    {
        if(_initialized) return;

        _cfg.OnValueChanged(CVars.UtkaSocketKey, value => _key = value, true);

        if(string.IsNullOrEmpty(_key)) return;

        _utkaSocket = new UtkaSocket(IPAddress.Any, 8888, _key);
        _utkaSocket.Start();

        _initialized = true;
    }

    public void Shutdown()
    {
        _utkaSocket?.Stop();
        _utkaSocket = null;

        _initialized = false;
    }
}
