namespace Protoinject
{
    public interface IBindToInScopeWithDescendantFilterOrUniqueOrNamed<TInterface> : IBindTo<TInterface>,
        IBindInScopeWithDescendantFilterOrUniqueOrNamed
    {
    }

    public interface IBindToInScopeWithDescendantFilterOrUniqueOrNamed : IBindToImplicit,
        IBindInScopeWithDescendantFilterOrUniqueOrNamed
    {
    }
}