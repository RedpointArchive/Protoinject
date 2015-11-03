using System;
using System.Collections.Generic;

namespace Protoinject
{
    public interface INode
    {
        INode Parent { get; }

        string Name { get; set; }

        IReadOnlyCollection<INode> Children { get; }
        
        string FullName { get; }

        object Value { get; }

        Type Type { get; }

        T GetValue<T>();

        IReadOnlyCollection<INode> GetParents();
    }
}