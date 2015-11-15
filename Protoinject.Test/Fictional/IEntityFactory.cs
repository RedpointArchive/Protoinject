using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protoinject.Example
{
    public interface IEntityFactory : IGenerateFactory
    {
        IPlayer CreatePlayer(string name);
        INetworkingPlayer CreateNetworkingPlayer();
    }
}
