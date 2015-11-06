using System;

namespace Protoinject.Example
{
    public class DefaultWorld : IWorld
    {
        public DefaultWorld(
            IProfiler profiler,
            INetworkingSession networkSession,
            Func<string, Player> playerFactory,
            ICurrentNode currentNode)
        {
            currentNode.SetName("AmazingWorld");

            playerFactory("Player1");
            playerFactory("Player2");
        }
    }
}