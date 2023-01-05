using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Content.Server.Utility;
using NetCoreServer;
using Robust.Shared.Asynchronous;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaSocket : UdpServer
{
    public static Dictionary<string, IUtkaCommand> Commands = new();
    private readonly string Key = string.Empty;
    private readonly ISawmill _sawmill = default!;
    private readonly ITaskManager _taskManager = default!;

    public List<EndPoint> EndPoints = new();


    public UtkaSocket(IPAddress address, int port, string key) : base(address, port)
    {
        Key = key;
        _sawmill = Logger.GetSawmill("utkasockets");
        _taskManager = IoCManager.Resolve<ITaskManager>();
        RegisterCommands();
    }

    protected override void OnStarted()
    {
        base.OnStarted();
        ReceiveAsync();
    }

    protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
    {
        base.OnReceived(endpoint, buffer, offset, size);

        var message = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);

        var fromDiscordMessage = JsonSerializer.Deserialize<FromDiscordMessage>(message);

        if (!NullCheck(fromDiscordMessage!))
        {
            _sawmill.Info($"UTKASockets: Received message from discord, but it was cringe.");
            return;
        }

        if (fromDiscordMessage!.Key != Key)
        {
            _sawmill.Info($"UTKASockets: Received message with invalid key from endpoint {endpoint}");
            return;
        }

        if (!EndPoints.Contains(endpoint))
        {
            EndPoints.Add(endpoint);
        }


        ExecuteCommand(endpoint, fromDiscordMessage, fromDiscordMessage!.Command!, fromDiscordMessage!.Message!.ToArray());

        ReceiveAsync();
    }

    public void SendMessage(ToUtkaMessage message)
    {
        foreach (var endPoint in EndPoints)
        {
            var finalMessage = JsonSerializer.Serialize(message);
            SendAsync(endPoint, finalMessage);
        }
    }


    private void ExecuteCommand(EndPoint requester, FromDiscordMessage message, string command, string[] args)
    {
        if (!Commands.ContainsKey(command))
        {
            _sawmill.Warning($"UTKASockets: FAIL! Command {command} not found");
            return;
        }

        _sawmill.Info($"UTKASockets: Execiting command from UTKASocket: {command} args: {string.Join(" ", args)}");
        _taskManager.RunOnMainThread(() => Commands[command].Execute(this, requester, message, args));
    }

    private bool NullCheck(FromDiscordMessage fromDiscordMessage)
    {
        return fromDiscordMessage is {Key: { }, Ckey: { }, Message: { }, Command: { }};
    }

    protected override void OnSent(EndPoint endpoint, long sent)
    {
        base.OnSent(endpoint, sent);
        ReceiveAsync();
    }

    protected override void OnError(SocketError error)
    {
        base.OnError(error);

        _sawmill.Warning($"UTKA SOKETS FAIL! {error}");
    }


    public static void RegisterCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        var commands = types.Where(type => typeof(IUtkaCommand).IsAssignableFrom(type) && type.GetInterfaces().Contains(typeof(IUtkaCommand))).ToList();

        foreach (var command in commands)
        {
            if (Activator.CreateInstance(command) is IUtkaCommand utkaCommand)
            {
                Commands[utkaCommand.Name] = utkaCommand;
            }
        }
    }
}
