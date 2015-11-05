using System;

namespace Protoinject
{
    public interface IBindToInScopeWithDescendantFilterOrUnique<TInterface> : IBindTo<TInterface>,
        IBindInScopeWithDescendantFilterOrUnique
    {
    }

    public interface IBindTo<TInterface>
    {
        IBindInScopeWithDescendantFilterOrUnique To<T>() where T : TInterface;
        IBindInScopeWithDescendantFilterOrUnique To(Type type);
    }

    public interface IBindInScope
    {
        void InTransientScope();
        IBindUnique InScope(IScope scope);
    }

    public interface IBindUnique
    {
        void EnforceOnePerScope();
    }

    public interface IBindWithDescendantFilter
    {
        IBindInScopeOrUnique WithDescendantFilter(INode descendantOf);
    }

    public interface IBindInScopeWithDescendantFilterOrUnique : IBindInScopeOrUnique, IBindWithDescendantFilter
    {
    }

    public interface IBindInScopeOrUnique : IBindInScope, IBindUnique
    {
    }
}