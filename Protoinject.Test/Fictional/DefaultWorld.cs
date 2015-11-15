using System;

namespace Protoinject.Example
{
    public class DefaultWorld : IWorld
    {
        public DefaultWorld(
            IProfiler profiler,
            INetworkingSession networkSession,
            IEntityFactory playerFactory,
            ICurrentNode currentNode)
        {
            currentNode.SetName("AmazingWorld");

            playerFactory.CreatePlayer("Player1");
            playerFactory.CreatePlayer("Player2");
        }
    }
}