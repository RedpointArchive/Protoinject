using System.Collections.Generic;

namespace Protoinject
{
    public interface IHierarchy
    {
        IReadOnlyCollection<INode> RootNodes { get; }

        INode Lookup(object obj);

        void AddRootNode(INode node);

        void AddChildNode(IPlan parent, INode child);

        void RemoveRootNode(INode node);

        void RemoveChildNode(IPlan parent, INode child);

        void RemoveNode(INode node);

        void ChangeObjectOnNode(INode node, object newValue);

        INode CreateNodeForObject(object obj);
    }
}