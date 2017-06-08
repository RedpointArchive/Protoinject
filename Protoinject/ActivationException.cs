using System;
using System.Runtime.Serialization;

namespace Protoinject
{
    [Serializable]
    public class ActivationException : Exception
    {
        public ActivationException(string message, IPlan current) : base(message + " while underneath " + (current == null ? "<root>" : ("'" + current.FullName + "'")) + "")
        {
        }

        public ActivationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}