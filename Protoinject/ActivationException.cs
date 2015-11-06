using System;

namespace Protoinject
{
    public class ActivationException : Exception
    {
        public ActivationException(string message, IPlan current) : base(message + " while underneath " + (current == null ? "<root>" : ("'" + current.FullName + "'")) + "")
        {
        }
    }
}