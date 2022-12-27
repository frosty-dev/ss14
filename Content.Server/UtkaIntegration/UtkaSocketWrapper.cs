﻿using Content.Shared.CCVar;
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

        _cfg.OnValueChanged(CCVars.UtkaSocketKey, value => _key = value, true);

        if(string.IsNullOrEmpty(_key)) return;

        var port = _cfg.GetCVar(CVars.NetPort) + 100;

        try
        {
            _utkaSocket = new UtkaSocket("0.0.0.0", port, _key);

        }
        catch (Exception e)
        {
            Logger.GetSawmill("utkasockets").Error($"Failed to initialize UtkaSocket: {e}");
            return;
        }

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
