using System.Collections.Generic;

namespace Protoinject
{
    public interface IHierarchy
    {
        IReadOnlyCollection<INode> RootNodes { get; }
    }
}