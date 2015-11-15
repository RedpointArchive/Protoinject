using System.Reflection;

namespace Protoinject
{
    public class AbsolutePositionalConstructorArgument : IConstructorArgument
    {
        private readonly int _position;
        private readonly object _value;

        public AbsolutePositionalConstructorArgument(int position, object value)
        {
            _position = position;
            _value = value;
        }

        public bool Satisifies(ConstructorInfo constructor, ParameterInfo parameter)
        {
            return parameter.Position == _position;
        }

        public object GetValue()
        {
            return _value;
        }
    }
}