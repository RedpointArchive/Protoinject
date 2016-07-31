using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoinject
{
    internal class DefaultHierarchy : IHierarchy
    {
        private List<INode> _rootNodes;

        private Dictionary<object, List<INode>> _lookupCache;

        private HashSet<INode> _nodesTrackedInHierarchy;

        public DefaultHierarchy()
        {
            _rootNodes = new List<INode>();
            _lookupCache = new Dictionary<object, List<INode>>();
            _nodesTrackedInHierarchy = new HashSet<INode>();
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
            if (!((INode) parent).Children.Contains(child))
            {
                ((DefaultNode) parent).AddChild(child);
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
            ((DefaultNode)parent).RemoveChild(child);
            RemoveNodeFromLookup(child);
        }

        public void RemoveNode(INode node)
        {
            if (_rootNodes.Contains(node))
            {
                RemoveRootNode(node);
            }
            else if (node.Parent != null)
            {
                RemoveChildNode(node.Parent, node);
            }
        }

        public void ChangeObjectOnNode(INode node, object newValue)
        {
            if (((DefaultNode) node).Type == null)
            {
                throw new InvalidOperationException("You can't change the object on this node, because no type has been assigned to this node.");
            }
            if (!((DefaultNode) node).Type.IsInstanceOfType(newValue))
            {
                throw new InvalidOperationException("The passed value needs to be an instance of or derive from " + ((DefaultNode)node).Type.FullName + ", but it does not.");
            }

            RemoveNodeFromLookup(node);
            ((DefaultNode)node).UntypedValue = newValue;
            AddNodeToLookup(node);
        }

        public INode CreateNodeForObject(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return new DefaultNode
            {
                UntypedValue = obj,
                Type = obj.GetType(),
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