namespace Protoinject.Example
{
    public interface IEntityFactory : IGenerateFactory
    {
        IPlayer CreatePlayer(string name);
        INetworkingPlayer CreateNetworkingPlayer();
    }
}
