using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protoinject.Example
{
    /// <summary>
    /// For reference inside ILSpy.
    /// </summary>
    public class ReferenceEntityFactory : IEntityFactory
    {
        private readonly INode _node;
        private readonly IKernel _kernel;

        public ReferenceEntityFactory(ICurrentNode node, IKernel kernel)
        {
            _node = node.GetNodeForFactoryImplementation();
            _kernel = kernel;
        }

        public IPlayer CreatePlayer(string name)
        {
            return (IPlayer)_kernel.Get(
                typeof (IPlayer),
                _node,
                null,
                null,
                new NamedConstructorArgument("name", name));
        }

        public INetworkingPlayer CreateNetworkingPlayer()
        {
            return (INetworkingPlayer)_kernel.Get(
                typeof(INetworkingPlayer),
                _node,
                null,
                null);
        }
    }
}
