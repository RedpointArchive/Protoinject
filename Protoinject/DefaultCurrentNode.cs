namespace Protoinject
{
    internal class DefaultCurrentNode : ICurrentNode
    {
        private readonly INode _target;

        public DefaultCurrentNode(INode target)
        {
            _target = target;
        }

        public void SetName(string name)
        {
            _target.Name = name;
        }
    }
}