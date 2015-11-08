using System.Reflection;

namespace Protoinject
{
    public class RelativePositionalConstructorArgument : IConstructorArgument
    {
        private readonly int _position;
        private readonly object _value;

        public RelativePositionalConstructorArgument(int position, object value)
        {
            _position = position;
            _value = value;
        }

        public bool Satisifies(ConstructorInfo constructor, ParameterInfo parameter, int relativePosition)
        {
            return relativePosition == _position;
        }

        public object GetValue()
        {
            return _value;
        }
    }
}