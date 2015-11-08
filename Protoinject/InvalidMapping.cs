using System;

namespace Protoinject
{
    internal class InvalidMapping : IMapping
    {
        public Type Target { get; }
        public Func<IContext, object> TargetMethod { get; }
        public INode OnlyUnderDescendantFilter { get; }
        public IScope LifetimeScope { get; }
        public bool UniquePerScope { get; }
        public bool Valid => false;
        public string Named { get; }
    }
}