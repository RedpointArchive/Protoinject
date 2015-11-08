namespace Protoinject
{
    public interface IContext
    {
        INode Parent { get; }

        IPlan ChildToResolve { get; }
    }
}