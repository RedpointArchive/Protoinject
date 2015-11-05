using System;

namespace Protoinject
{
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
        public string ParameterName { get; set; }
    }
}