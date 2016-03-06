using System.Collections.Generic;
using System.Linq;

namespace Protoinject
{
    internal class DefaultHierarchy : IHierarchy
    {
        private List<INode> _rootNodes;

        private Dictionary<object, List<INode>> _lookupCache;

        private List<INode> _nodesTrackedInHierarchy;

        public DefaultHierarchy()
        {
            _rootNodes = new List<INode>();
            _lookupCache = new Dictionary<object, List<INode>>();
            _nodesTrackedInHierarchy = new List<INode>();
        }

        IReadOnlyCollection<INode> IHierarchy.RootNodes => _rootNodes.AsReadOnly();

        public INode Lookup(object obj)
        {
            return _lookupCache.ContainsKey(obj) ? _lookupCache[obj].FirstOrDefault() : null;
        }

        public void AddRootNode(INode node)
        {
            _rootNodes.Add(node);
            AddNodeToLookup(node);
        }

        public void AddChildNode(IPlan parent, INode child)
        {
            var children = ((DefaultNode)parent).ChildrenInternal;
            if (!children.Contains(child))
            {
                children.Add(child);
            }
            AddNodeToLookup(child);
        }

        public void RemoveRootNode(INode node)
        {
            _rootNodes.Remove(node);
            RemoveNodeFromLookup(node);
        }

        public void RemoveChildNode(IPlan parent, INode child)
        {
            ((DefaultNode)parent).ChildrenInternal.Remove(child);
            RemoveNodeFromLookup(child);
        }

        public void ChangeObjectOnNode(INode node, object newValue)
        {
            RemoveNodeFromLookup(node);
            ((DefaultNode)node).UntypedValue = newValue;
            AddNodeToLookup(node);
        }

        public INode CreateNodeForObject(object obj)
        {
            return new DefaultNode
            {
                UntypedValue = obj,
                Discarded = false,
            };
        }

        private void AddNodeToLookup(INode node)
        {
            if (!_nodesTrackedInHierarchy.Contains(node))
            {
                _nodesTrackedInHierarchy.Add(node);
                if (node.UntypedValue != null)
                {
                    if (!_lookupCache.ContainsKey(node.UntypedValue))
                    {
                        _lookupCache[node.UntypedValue] = new List<INode>();
                    }

                    _lookupCache[node.UntypedValue].Add(node);
                }
            }
            foreach (var child in node.Children)
            {
                AddNodeToLookup(child);
            }
        }

        private void RemoveNodeFromLookup(INode node)
        {
            if (_nodesTrackedInHierarchy.Contains(node))
            {
                _nodesTrackedInHierarchy.Remove(node);
                if (node.UntypedValue != null)
                {
                    if (!_lookupCache.ContainsKey(node.UntypedValue))
                    {
                        _lookupCache[node.UntypedValue] = new List<INode>();
                    }
                    
                    _lookupCache[node.UntypedValue].Remove(node);
                }
            }
            foreach (var child in node.Children)
            {
                RemoveNodeFromLookup(child);
            }
        }
    }
}