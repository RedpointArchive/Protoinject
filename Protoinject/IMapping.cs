using System;

namespace Protoinject
{
    public interface IMapping
    {
        Type Target { get; }

        Func<IContext, object> TargetMethod { get; }

        bool TargetFactory { get; }

        bool TargetFactoryNotSupported { get; }

        INode OnlyUnderDescendantFilter { get; }

        IScope LifetimeScope { get; }

        bool UniquePerScope { get; }

        bool DiscardNodeOnResolve { get; }

        string Named { get; }

        bool Valid { get; }
    }
}