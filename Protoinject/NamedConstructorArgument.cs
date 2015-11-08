using System.Reflection;

namespace Protoinject
{
    public class NamedConstructorArgument : IConstructorArgument
    {
        private readonly string _name;
        private readonly object _value;

        public NamedConstructorArgument(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public bool Satisifies(ConstructorInfo constructor, ParameterInfo parameter, int relativePosition)
        {
            return parameter.Name == _name;
        }

        public object GetValue()
        {
            return _value;
        }
    }
}