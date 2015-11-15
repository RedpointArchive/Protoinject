namespace Protoinject
{
    public class DefaultContext : IContext
    {
        public DefaultContext(IKernel kernel, INode parent, IPlan childToResolve)
        {
            Kernel = kernel;
            Parent = parent;
            ChildToResolve = childToResolve;
        }

        public IKernel Kernel { get; }
        public INode Parent { get; }
        public IPlan ChildToResolve { get; }
    }
}