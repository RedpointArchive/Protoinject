using System.Collections.Generic;

namespace Protoinject
{
    internal interface IWritableHierarchy : IHierarchy
    {
        new List<INode> RootNodes { get; }
    }

    internal class DefaultHierarchy : IWritableHierarchy
    {
        private List<INode> _rootNodes;

        public DefaultHierarchy()
        {
            _rootNodes = new List<INode>();
        }

        IReadOnlyCollection<INode> IHierarchy.RootNodes => _rootNodes.AsReadOnly();

        public List<INode> RootNodes => _rootNodes;
    }
}