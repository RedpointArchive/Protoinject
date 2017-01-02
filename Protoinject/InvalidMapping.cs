using System;

namespace Protoinject
{
    internal class InvalidMapping : IMapping
    {
        public Type Target { get; private set; }
        public Func<IContext, object> TargetMethod { get; private set; }
        public bool TargetFactory { get; private set; }
        public bool TargetFactoryNotSupported { get; private set; }
        public INode OnlyUnderDescendantFilter { get; private set; }
        public IScope LifetimeScope { get; private set; }
        public bool UniquePerScope { get; private set; }
        public bool DiscardNodeOnResolve { get; private set; }
        public bool Valid { get { return false; } }
        public string Named { get; private set; }
    }
}