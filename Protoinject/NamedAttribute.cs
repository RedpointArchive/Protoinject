namespace Protoinject
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class NamedAttribute : Attribute
    {
        public NamedAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}