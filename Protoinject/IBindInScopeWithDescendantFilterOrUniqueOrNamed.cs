namespace Protoinject
{
    public interface IBindInScopeWithDescendantFilterOrUniqueOrNamed : IBindInScopeWithDescendantFilterOrUnique
    {
        IBindInScopeWithDescendantFilterOrUnique Named(string name);
    }
}