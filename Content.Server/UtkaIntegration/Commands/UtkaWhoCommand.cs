using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaWhoCommand : IUtkaCommand
{
    public string Name => "who";

    public void Execute(UtkaSocket socket, EndPoint requester, FromDiscordMessage message, string[] args)
    {
        var configManager = IoCManager.Resolve<IConfigurationManager>();

        var players = Filter.GetAllPlayers().ToList();
        var playerNames = players
            .Where(player => player.Status != SessionStatus.Disconnected)
            .Select(x => x.Name);

        StringBuilder builder = new();

        foreach (var playerName in playerNames)
        {
            builder.AppendLine(playerName);
        }

        var toUtkaMessage = new ToUtkaMessage()
        {
            Key = configManager.GetCVar(CCVars.UtkaSocketKey),
            Command = Name,
            Message = new List<string>()
            {
                players.Count.ToString(),
                builder.ToString()
            }
        };

        var finalMessage = JsonSerializer.Serialize(toUtkaMessage);

        socket.SendAsync(requester, finalMessage);
    }
}
