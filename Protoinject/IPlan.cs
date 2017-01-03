using System;
using System.Collections.Generic;
#if PLATFORM_UNITY
using System.Collections.ObjectModel;
#endif
using System.Net.Configuration;
using System.Reflection;

namespace Protoinject
{
    public interface IPlan
    {
        bool Planned { get; }

        bool Deferred { get; }

#if !PLATFORM_UNITY
        IReadOnlyCollection<KeyValuePair<Type, INode>> DeferredSearchOptions { get; }
#else
        Dictionary<Type, INode> DeferredSearchOptions { get; }
#endif

        INode DeferredResolvedTarget { get; }

        string PlanName { get; }

        IPlan ParentPlan { get; }

#if !PLATFORM_UNITY
        IReadOnlyCollection<IPlan> ChildrenPlan { get; }
#else
        ReadOnlyCollection<INode> ChildrenPlan { get; }
#endif

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