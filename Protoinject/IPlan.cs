using System.Collections.Generic;
using System.Reflection;

namespace Protoinject
{
    public interface IPlan
    {
        IPlan ParentPlan { get; }

        IReadOnlyCollection<IPlan> ChildrenPlan { get; }

        ConstructorInfo PlannedConstructor { get; }

        List<IUnresolvedArgument> PlannedConstructorArguments { get; }

        string FullName { get; }
    }

    public interface IPlan<out T> : IPlan
    {
    }
}