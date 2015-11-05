namespace Protoinject.Example
{
    public class DefaultMovement : IMovement
    {
        public DefaultMovement(IInput input, ICurrentNode currentNode)
        {
            currentNode.SetName(Program.GetRandomName());
        }
    }
}