namespace Protoinject
{
    public interface IHierarchy
    {
#if PLATFORM_UNITY
        System.Collections.ObjectModel.ReadOnlyCollection<INode> RootNodes { get; }
#else
        System.Collections.Generic.IReadOnlyCollection<INode> RootNodes { get; }
#endif

        int LookupCacheObjectCount { get; }

        INode Lookup(object obj);

        void AddRootNode(INode node);

        void AddChildNode(IPlan parent, INode child);

        void MoveNode(IPlan newParent, INode child);

        void RemoveRootNode(INode node);

        void RemoveChildNode(IPlan parent, INode child);

        void RemoveNode(INode node);

        void ChangeObjectOnNode(INode node, object newValue);

        INode CreateNodeForObject(object obj);
    }
}