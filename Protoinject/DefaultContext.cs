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

        public IKernel Kernel { get; private set; }
        public INode Parent { get; private set; }
        public IPlan ChildToResolve { get; private set; }
    }
}