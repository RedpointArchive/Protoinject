using System;
using System.Collections.Generic;
#if PLATFORM_UNITY
using System.Collections.ObjectModel;
#endif

namespace Protoinject
{
    public interface INode : IPlan
    {
        INode Parent { get; }

        string Name { get; set; }

#if PLATFORM_UNITY
        ReadOnlyCollection<INode> Children { get; }
#else
        IReadOnlyCollection<INode> Children { get; }
#endif

        object UntypedValue { get; }

        Type Type { get; }

#if PLATFORM_UNITY
        ReadOnlyCollection<INode> GetParents();
#else
        IReadOnlyCollection<INode> GetParents();
#endif

        event ValueChangedEventHandler ValueChanged;

        event EventHandler ChildrenChanged;

        event EventHandler DescendantsChanged;
    }

    public interface INode<out T> : INode, IPlan<T>
    {
        T Value { get; }
    }
}