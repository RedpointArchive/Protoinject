using System.Collections.Generic;

namespace Protoinject
{
    internal class DefaultHierarchy : IHierarchy
    {
        private List<INode> _rootNodes;

        public DefaultHierarchy()
        {
            _rootNodes = new List<INode>();
        }

        public IReadOnlyCollection<INode> RootNodes => _rootNodes.AsReadOnly();

        public void AddRootNode(INode node)
        {
            _rootNodes.Add(node);
        }
    }
}