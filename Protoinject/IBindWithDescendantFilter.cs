namespace Protoinject
{
    public interface IBindWithDescendantFilter
    {
        IBindInScopeOrUnique WithDescendantFilter(INode descendantOf);
    }
}