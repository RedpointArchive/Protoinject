using System;

namespace Protoinject
{
    public interface IMapping
    {
        Type Target { get; }

        INode OnlyUnderDescendantFilter { get; }

        IScope LifetimeScope { get; }

        bool Reuse { get; }
    }
}