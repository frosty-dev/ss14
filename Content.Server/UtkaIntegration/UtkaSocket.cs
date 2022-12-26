using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetCoreServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaSocket : UdpServer
{
    public static Dictionary<string, IUtkaCommand> Commands = new();
    private static string Key = "UtkaKey";

    public UtkaSocket(IPAddress address, int port) : base(address, port) { }

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

        ExecuteCommand(fromDiscordMessage!.Command!, fromDiscordMessage!.Message!.ToArray());
    }


    private void ExecuteCommand(string command, string[] args)
    {
        if (Commands.ContainsKey(command))
        {
            var sawmill = IoCManager.Resolve<ISawmill>();

            sawmill.Warning($"UTKA SOKETS FAIL! Command {command} not found");
            return;
        }

        Commands[command].Execute(this, args);
    }

    protected override void OnSent(EndPoint endpoint, long sent)
    {
        base.OnSent(endpoint, sent);
        ReceiveAsync();
    }

    protected override void OnError(SocketError error)
    {
        base.OnError(error);

        var sawmill = IoCManager.Resolve<ISawmill>();
        sawmill.Warning($"UTKA SOKETS FAIL! Blyat! {error}");
    }


    public static void RegisterCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        var commands = types.Where(type => typeof(IUtkaCommand).IsAssignableFrom(type) && type.GetInterfaces().Contains(typeof(IUtkaCommand))).ToList();

        foreach (var command in commands)
        {
            var utkaCommand = Activator.CreateInstance(command) as IUtkaCommand;
            Commands.Add(utkaCommand!.Name, utkaCommand);
        }
    }
}
