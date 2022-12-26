namespace Content.Server.UtkaIntegration;

public interface IUtkaCommand
{
    string Name { get; }

    public async void Execute(UtkaSocket socket, string[] args)
    {

    }
}
