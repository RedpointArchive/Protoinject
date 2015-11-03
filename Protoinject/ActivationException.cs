using System;

namespace Protoinject
{
    public class ActivationException : Exception
    {
        public ActivationException(string message, INode current) : base(message + " while underneath '" + current.FullName + "'")
        {
        }
    }
}