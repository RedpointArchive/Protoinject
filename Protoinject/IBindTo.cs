using System;

namespace Protoinject
{
    public interface IBindTo
    {
        IBindInScopeWithDescendantFilterOrUniqueOrNamed To(Type type);
        IBindInScopeWithDescendantFilterOrUniqueOrNamed ToMethod(Func<IContext, object> resolve);
        IBindInScopeWithDescendantFilterOrUniqueOrNamed ToFactory();
        IBindInScopeWithDescendantFilterOrUniqueOrNamed ToFactoryNotSupported();
    }

    public interface IBindToImplicit : IBindTo
    {
        IBindInScopeWithDescendantFilterOrUniqueOrNamed To<T>();
    }

    public interface IBindTo<TInterface> : IBindTo
    {
        IBindInScopeWithDescendantFilterOrUniqueOrNamed To<T>() where T : TInterface;
    }
}