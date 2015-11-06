using System;
using System.Collections.Generic;

namespace Protoinject
{
    public interface INode : IPlan
    {
        INode Parent { get; }

        string Name { get; set; }

        IReadOnlyCollection<INode> Children { get; }

        object UntypedValue { get; }

        Type Type { get; }

        IReadOnlyCollection<INode> GetParents();
    }

    public interface INode<out T> : INode, IPlan<T>
    {
        T Value { get; }
    }
}