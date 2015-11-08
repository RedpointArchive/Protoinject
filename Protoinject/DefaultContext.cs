namespace Protoinject
{
    public class DefaultContext : IContext
    {
        public DefaultContext(INode parent, IPlan childToResolve)
        {
            Parent = parent;
            ChildToResolve = childToResolve;
        }

        public INode Parent { get; }
        public IPlan ChildToResolve { get; }
    }
}