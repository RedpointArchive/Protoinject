using System;

namespace Protoinject.Example
{
    public class DefaultMovement : IMovement
    {
        public DefaultMovement(IInput input, ICurrentNode currentNode)
        {
            currentNode.SetName(GetRandomName());
        }

        private static Random random = new Random();

        public static string GetRandomName()
        {
            return "inst" + random.Next();
        }
    }
}