namespace Protoinject
{
    internal class FixedScope : IScope
    {
        private readonly INode _node;

        public FixedScope(INode node)
        {
            _node = node;
        }

        public INode GetContainingNode()
        {
            return _node;
        }
    }
}