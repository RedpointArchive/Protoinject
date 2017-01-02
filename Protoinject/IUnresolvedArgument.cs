using System;
using System.Collections.Generic;

namespace Protoinject
{
    public interface IUnresolvedArgument
    {
        UnresolvedArgumentType ArgumentType { get; }
        Type UnresolvedType { get; }
        Type FactoryType { get; }
        int FactoryArgumentPosition { get; }
        ICurrentNode CurrentNode { get; }
        object FactoryArgumentValue { get; }
        Delegate FactoryDelegate { get; }
        IPlan PlannedTarget { get; }
#if !PLATFORM_UNITY
        IPlan[] PlannedTargets { get; }
#else
        List<IPlan> PlannedTargets { get; }
#endif
        object KnownValue { get; }
        bool IsMultipleResult { get; }
        Type MultipleResultElementType { get; }
        IInjectionAttribute[] InjectionParameters { get; }
        INode Node { get; }
        string Name { get; set; }
        IHierarchy Hierarchy { get; }
    }
}