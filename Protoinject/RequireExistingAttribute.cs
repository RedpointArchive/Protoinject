using System;

namespace Protoinject
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequireExistingAttribute : Attribute, IInjectionAttribute
    {
    }
}
