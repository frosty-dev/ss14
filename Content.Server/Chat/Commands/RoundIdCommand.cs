using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.GameTicking;

namespace Content.Server.Chat.Commands
{
    [AnyCommand]
    internal sealed class RoundId : IConsoleCommand
    {
        public string Command => "roundid";
        public string Description => "Shows the id of the current round.";
        public string Help => "Write roundid, output *Current round #roundId*";

        private int _roundId => EntitySystem.Get<GameTicker>().RoundId;

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            shell.WriteLine($"Current round #{_roundId}");
        }
    }
}
