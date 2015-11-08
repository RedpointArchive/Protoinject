namespace Protoinject
{
    public interface IBindInScope
    {
        void InTransientScope();
        IBindUnique InSingletonScope();
        IBindUnique InScope(IScope scope);
    }
}