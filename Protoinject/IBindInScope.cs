namespace Protoinject
{
    public interface IBindInScope
    {
        IBindUnique DiscardNodeOnResolve();
        IBindUnique InParentScope();
        IBindUnique InSingletonScope();
        IBindUnique InScope(IScope scope);
    }
}