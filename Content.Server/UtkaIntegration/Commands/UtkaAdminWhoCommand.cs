using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Content.Server.Administration.Managers;
using Content.Shared.CCVar;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Server.UtkaIntegration;

public sealed class UtkaAdminWhoCommand : IUtkaCommand
{
    public string Name => "adminwho";
    public void Execute(UtkaSocket socket, EndPoint requester, FromDiscordMessage message, string[] args)
    {
        var adminManager = IoCManager.Resolve<IAdminManager>();
        var configManager = IoCManager.Resolve<IConfigurationManager>();

        var admins = adminManager.ActiveAdmins.ToList();
        var builder = new StringBuilder();

        foreach (var admin in admins)
        {
            builder.AppendLine(admin.Name);
        }

        var toUtkaMessage = new ToUtkaMessage()
        {
            Key = configManager.GetCVar(CCVars.UtkaSocketKey),
            Command = Name,
            Message = new List<string>()
            {
                admins.Count.ToString(),
                builder.ToString()
            }
        };

        var finalMessage = JsonSerializer.Serialize(toUtkaMessage);

        socket.SendAsync(requester, finalMessage);
    }
}
