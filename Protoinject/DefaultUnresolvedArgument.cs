using System;

namespace Protoinject
{
    using System.Collections.Generic;

    internal class DefaultUnresolvedArgument : IUnresolvedArgument
    {
        public UnresolvedArgumentType ArgumentType { get; set; }
        public Type UnresolvedType { get; set; }
        public Type FactoryType { get; set; }
        public int FactoryArgumentPosition { get; private set; }
        public ICurrentNode CurrentNode { get; set; }
        public object FactoryArgumentValue { get; set; }
        public Delegate FactoryDelegate { get; set; }
        public IPlan PlannedTarget { get; set; }
#if !PLATFORM_UNITY
        public IPlan[] PlannedTargets { get; set; }
#else
        public List<IPlan> PlannedTargets { get; set; }
#endif
        public object KnownValue { get; set; }
        public IInjectionAttribute[] InjectionParameters { get; set; }
        public INode Node { get; set; }
        public string Name { get; set; }
        public IHierarchy Hierarchy { get; set; }

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
#if !PLATFORM_UNITY
                    return UnresolvedType.GenericTypeArguments[0];
#else
                    return UnresolvedType.GetGenericArguments()[0];
#endif
                }

                throw new NotSupportedException();
            }
        }
    }
}