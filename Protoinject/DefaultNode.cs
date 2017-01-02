using System;
using System.Collections.Generic;
#if PLATFORM_UNITY
using System.Collections.ObjectModel;
#endif
using System.Linq;
using System.Reflection;

namespace Protoinject
{
    internal class DefaultNode<T> : DefaultNode, INode<T>
    {
        private T _cachedValue;

        public T Value
        {
            get
            {
                if (_valueChanged)
                {
                    _cachedValue = (T) UntypedValue;
                    _valueChanged = false;
                }

                return _cachedValue;
            }
        }
    }

    internal class DefaultNode : INode
    {
        private readonly List<INode> _childrenInternal;

        public DefaultNode()
        {
            _childrenInternal = new List<INode>();
            PlannedCreatedNodes = new List<IPlan>();
            DeferredCreatedNodes = new List<IPlan>();
            DeferredSearchOptions = new Dictionary<Type, INode>();
            DependentOnPlans = new List<IPlan>();
            DiscardOnResolve = new List<IPlan>();
            _valueChanged = true;
        }

        public List<IPlan> DiscardOnResolve { get; set; }

        protected bool _valueChanged;
        private object _untypedValue;

        internal void AddChild(INode node)
        {
            _childrenInternal.Add(node);
            if (ChildrenChanged != null)
            {
                ChildrenChanged.Invoke(this, new EventArgs());
            }

            var a = this;
            while (a != null)
            {
                if (a.DescendantsChanged != null)
                {
                    a.DescendantsChanged.Invoke(this, new EventArgs());
                }
                a = (DefaultNode) a.Parent;
            }
        }

        internal void RemoveChild(INode node)
        {
            _childrenInternal.Remove(node);
            if (ChildrenChanged != null)
            {
                ChildrenChanged.Invoke(this, new EventArgs());
            }

            var a = this;
            while (a != null)
            {
                if (a.DescendantsChanged != null)
                {
                    a.DescendantsChanged.Invoke(this, new EventArgs());
                }
                a = (DefaultNode)a.Parent;
            }
        }

        public INode Parent { get; set; }
        public string Name { get; set; }

#if PLATFORM_UNITY
        public ReadOnlyCollection<INode> Children
#else
        public IReadOnlyCollection<INode> Children
#endif
        {
            get
            {
                return _childrenInternal.AsReadOnly();
            }
        }
        
        public event ValueChangedEventHandler ValueChanged;
        public event EventHandler ChildrenChanged;
        public event EventHandler DescendantsChanged;

        public List<IPlan> PlannedCreatedNodes { get; private set; }
        public List<IPlan> DeferredCreatedNodes { get; private set; }

        public string FullName
        {
            get
            {
                return
                    GetParents()
                        .Concat(new[] {this})
                        .Select(x => string.IsNullOrEmpty(x.Name) ? (x.Type == null ? "(unknown)" : ("(" + x.Type.FullName + ")")) : x.Name)
                        .Aggregate((a, b) => a + "/" + b);
            }
        }

        public IPlan PlanRoot { get; set; }
        public List<IPlan> DependentOnPlans { get; private set; }
        public bool Discarded { get; set; }

        public bool Valid
        {
            get
            {
                if (this.Planned)
                {
                    return UntypedValue != null || PlannedConstructor != null || PlannedMethod != null || (Deferred && DeferredResolvedTarget != null);
                }

                return !Discarded;
            }
        }

        public object UntypedValue
        {
            get { return _untypedValue; }
            internal set
            {
                if (_untypedValue != value)
                {
                    var oldValue = _untypedValue;
                    _untypedValue = value;
                    _valueChanged = true;
                    if (ValueChanged != null)
                    {
                        ValueChanged.Invoke(this, new ValueChangedEventArgs { OldValue = oldValue, NewValue = _untypedValue });
                    }
                }
            }
        }

        public Type Type { get; set; }

#if PLATFORM_UNITY
        public ReadOnlyCollection<INode> GetParents()
#else
        public IReadOnlyCollection<INode> GetParents()
#endif
        {
            var parents = new List<INode>();
            var current = Parent;
            while (current != null)
            {
                parents.Insert(0, current);
                current = current.Parent;
            }
            return parents.AsReadOnly();
        }

        public bool Planned { get; set; }
        
        public string PlanName { get; set; }
        
        public IPlan ParentPlan
        {
            get { return Parent; }
        }

#if PLATFORM_UNITY
        public ReadOnlyCollection<INode> ChildrenPlan
#else
        public IReadOnlyCollection<IPlan> ChildrenPlan
#endif
        {
            get
            {
                return _childrenInternal.AsReadOnly();
            }
        }

        public ConstructorInfo PlannedConstructor { get; set; }

        public List<IUnresolvedArgument> PlannedConstructorArguments { get; set; }
        public string InvalidHint { get; set; }
        public Func<IContext, object> PlannedMethod { get; set; }

        public bool Deferred { get; set; }
#if PLATFORM_UNITY
        public Dictionary<Type, INode> DeferredSearchOptions { get; set; }
#else
        public IReadOnlyCollection<KeyValuePair<Type, INode>> DeferredSearchOptions { get; set; }
#endif
        public INode DeferredResolvedTarget { get; set; }
        public Type RequestedType { get; set; }

        public static string NormalizeName(string name)
        {
            return (name ?? string.Empty).Replace("/", "");
        }

        public override string ToString()
        {
            return this.GetDebugRepresentation();
        }
    }
}