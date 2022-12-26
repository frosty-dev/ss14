using System.Net;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaSocketWrapper
{
    private UtkaSocket? _utkaSocket;

    private bool _initialized;

    public void Initialize()
    {
        if(!_initialized) return;
        _utkaSocket = new UtkaSocket(IPAddress.Any, 8888);
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
