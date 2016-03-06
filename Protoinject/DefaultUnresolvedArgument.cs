using System;

namespace Protoinject
{
    using System.Collections.Generic;

    internal class DefaultUnresolvedArgument : IUnresolvedArgument
    {
        public UnresolvedArgumentType ArgumentType { get; set; }
        public Type UnresolvedType { get; set; }
        public Type FactoryType { get; set; }
        public int FactoryArgumentPosition { get; }
        public ICurrentNode CurrentNode { get; set; }
        public object FactoryArgumentValue { get; set; }
        public Delegate FactoryDelegate { get; set; }
        public IPlan PlannedTarget { get; set; }
        public IPlan[] PlannedTargets { get; set; }
        public object KnownValue { get; set; }
        public IInjectionAttribute[] InjectionParameters { get; set; }
        public INode Node { get; set; }
        public string Name { get; set; }

        public bool IsMultipleResult
        {
            get
            {
                return UnresolvedType.IsArray
                       || (UnresolvedType.IsGenericType
                           && UnresolvedType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }
        }

        public Type MultipleResultElementType
        {
            get
            {
                if (UnresolvedType.IsArray)
                {
                    return UnresolvedType.GetElementType();
                }
                else if (UnresolvedType.IsGenericType && UnresolvedType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return UnresolvedType.GenericTypeArguments[0];
                }

                throw new NotSupportedException();
            }
        }
    }
}