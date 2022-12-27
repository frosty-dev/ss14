namespace Content.Server.UtkaIntegration;

public interface IUtkaCommand
{
    string Name { get; }
    public void Execute(UtkaSocket socket, FromDiscordMessage message, string[] args);
}
