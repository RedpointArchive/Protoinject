namespace Protoinject
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class NamedAttribute : Attribute, IInjectionAttribute
    {
        public NamedAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}