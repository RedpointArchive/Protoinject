using System;

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
        IPlan[] PlannedTargets { get; }
        object KnownValue { get; }
        bool IsMultipleResult { get; }
        Type MultipleResultElementType { get; }
        IInjectionAttribute[] InjectionParameters { get; }
        INode Node { get; }
    }
}