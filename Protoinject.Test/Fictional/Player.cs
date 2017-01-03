using System;

namespace Protoinject.Example
{
    public class Player : Entity, IPlayer
    {
        private readonly string _name;

        public Player(IMovement movement, IEntityFactory networkPlayer, ICurrentNode currentNode, string name)
        {
            currentNode.SetName(name);
            networkPlayer.CreateNetworkingPlayer();

            _name = name;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}