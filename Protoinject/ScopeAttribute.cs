using System;

namespace Protoinject
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ScopeAttribute : Attribute, IInjectionAttribute
    {
        public abstract GetScopeFromContext ScopeFromContext { get; }

        public abstract bool UniquePerScope { get; }
    }
}