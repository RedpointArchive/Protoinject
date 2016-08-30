using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Reflection;

namespace Protoinject
{
    public interface IPlan
    {
        bool Planned { get; }

        bool Deferred { get; }

        IReadOnlyCollection<KeyValuePair<Type, INode>> DeferredSearchOptions { get; }

        INode DeferredResolvedTarget { get; }

        string PlanName { get; }

        IPlan ParentPlan { get; }

        IReadOnlyCollection<IPlan> ChildrenPlan { get; }

        ConstructorInfo PlannedConstructor { get; }

        List<IUnresolvedArgument> PlannedConstructorArguments { get; }

        Func<IContext, object> PlannedMethod { get; }

        List<IPlan> PlannedCreatedNodes { get; }

        List<IPlan> DeferredCreatedNodes { get; }

        string FullName { get; }

        IPlan PlanRoot { get; }

        List<IPlan> DependentOnPlans { get; } 

        bool Discarded { get; }

        bool Valid { get; }

        string InvalidHint { get; }

        Type RequestedType { get; }

        List<IPlan> DiscardOnResolve { get; }
    }

    public interface IPlan<out T> : IPlan
    {
    }
}