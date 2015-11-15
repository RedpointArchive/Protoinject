using System.Collections.Generic;
using System.Net.Configuration;
using System.Reflection;

namespace Protoinject
{
    public interface IPlan
    {
        bool Planned { get; }

        string PlanName { get; }

        IPlan ParentPlan { get; }

        IReadOnlyCollection<IPlan> ChildrenPlan { get; }

        ConstructorInfo PlannedConstructor { get; }

        List<IUnresolvedArgument> PlannedConstructorArguments { get; }

        List<IPlan> PlannedCreatedNodes { get; }

        string FullName { get; }

        IPlan PlanRoot { get; }

        List<IPlan> DependentOnPlans { get; } 

        bool Discarded { get; }

        bool Valid { get; }

        string InvalidHint { get; }
    }

    public interface IPlan<out T> : IPlan
    {
    }
}