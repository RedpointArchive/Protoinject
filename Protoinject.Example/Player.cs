using System;

namespace Protoinject.Example
{
    public class Player : Entity, IPlayer
    {
        public Player(IMovement movement, Func<INetworkingPlayer> networkPlayer, ICurrentNode currentNode, string name)
        {
            currentNode.SetName(name);
            networkPlayer();
        }
    }
}