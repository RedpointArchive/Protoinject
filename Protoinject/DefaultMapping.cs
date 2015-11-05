using System;

namespace Protoinject
{
    public class DefaultMapping : IMapping
    {
        public Type Target { get; internal set; }
        public INode OnlyUnderDescendantFilter { get; internal set; }
        public IScope LifetimeScope { get; internal set; }
        public bool UniquePerScope { get; internal set; }
    }
}
