using System.Reflection;

namespace Protoinject
{
    public interface IConstructorArgument
    {
        bool Satisifies(ConstructorInfo constructor, ParameterInfo parameter, int relativePosition);

        object GetValue();
    }
}