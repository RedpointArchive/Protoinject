#if PLATFORM_UNITY

using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoinject
{
    public partial class StandardKernel
    {
#region Get / TryGet / GetAll

        public T Get<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = Plan<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            Validate(plan);
            return Resolve(plan);
        }
        
        public object Get(Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments,
            Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = Plan(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            Validate(plan);
            return Resolve(plan);
        }

        public T TryGet<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments,
            Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = Plan<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            try
            {
                Validate(plan);
                return Resolve(plan);
            }
            catch (Exception)
            {
                Discard(plan);
                return (T)(object)null;
            }
        }

        public object TryGet(Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments,
            Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = Plan(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            try
            {
                Validate(plan);
                return Resolve(plan);
            }
            catch (Exception)
            {
                Discard(plan);
                return null;
            }
        }

        public T[] GetAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plans = PlanAll<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            ValidateAll(plans);
#if !PLATFORM_UNITY
            return ResolveAll(plans);
#else
            return ResolveAll(plans).ToArray();
#endif
        }
        public object[] GetAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plans = PlanAll(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            ValidateAll(plans);
#if !PLATFORM_UNITY
            return ResolveAll(plans);
#else
            return ResolveAll(plans).ToArray();
#endif
        }

        #endregion

        #region Planning

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>) Plan(typeof (T), current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public IPlan Plan(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return CreatePlan(type, current, bindingName, planName, null, injectionAttributes, arguments, transientBindings);
        }

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>)Plan(typeof(T), current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public IPlan Plan(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return CreatePlan(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }
        
        public List<IPlan<T>> PlanAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return PlanAll(typeof(T), current, bindingName, planName, injectionAttributes, arguments, transientBindings).Cast<IPlan<T>>().ToList();
        }

        public List<IPlan> PlanAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return CreatePlans(type, current, bindingName, planName, null, injectionAttributes, arguments, transientBindings);
        }

        public List<IPlan<T>> PlanAll<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return PlanAll(typeof(T), current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings).Cast<IPlan<T>>().ToList();
        }
        
        public List<IPlan> PlanAll(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return CreatePlans(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

#endregion

#region Validation

        public void Validate<T>(IPlan<T> plan)
        {
            Validate((IPlan)plan);
        }

        private void ValidateArgument(IUnresolvedArgument argument, IPlan target)
        {
            if (argument.InjectionParameters.OfType<OptionalAttribute>().Any())
            {
                // Optional arguments are always valid, because the result will
                // simply be null when injected.
                return;
            }

            if (!target.Valid)
            {
                throw new ActivationException("The planned node is not valid (hint: " + target.InvalidHint + ")", target);
            }
        }
        
        public void ValidateAll<T>(List<IPlan<T>> plans)
        {
            for (var i = 0; i < plans.Count; i++)
            {
                Validate(plans[i]);
            }
        }

        public void ValidateAll(List<IPlan> plans)
        {
            for (var i = 0; i < plans.Count; i++)
            {
                Validate(plans[i]);
            }
        }

#endregion

#region Resolution

        public INode<T> ResolveToNode<T>(IPlan<T> plan)
        {
            return (INode<T>)ResolveToNode((IPlan)plan);
        }
        
        public List<INode<T>> ResolveAllToNode<T>(List<IPlan<T>> plans)
        {
            var results = new List<INode<T>>(plans.Count);
            for (var i = 0; i < plans.Count; i++)
            {
                results.Add(ResolveToNode(plans[i]));
            }
            return results;
        }
        
        public List<INode> ResolveAllToNode(List<IPlan> plans)
        {
            var results = new List<INode>(plans.Count);
            for (var i = 0; i < plans.Count; i++)
            {
                results.Add(ResolveToNode(plans[i]));
            }
            return results;
        }

        public T Resolve<T>(IPlan<T> plan)
        {
            return ResolveToNode(plan).Value;
        }

        public object Resolve(IPlan plan)
        {
            return ResolveToNode(plan).UntypedValue;
        }

        public List<T> ResolveAll<T>(List<IPlan<T>> plans)
        {
            var results = new List<T>(plans.Count);
            for (var i = 0; i < plans.Count; i++)
            {
                results.Add(Resolve(plans[i]));
            }
            return results;
        }

        public List<object> ResolveAll(List<IPlan> plans)
        {
            var results = new List<object>(plans.Count);
            for (var i = 0; i < plans.Count; i++)
            {
                results.Add(Resolve(plans[i]));
            }
            return results;
        }

#endregion

#region Discard

        public void Discard<T>(IPlan<T> plan)
        {
            Discard((IPlan)plan);
        }

        public void DiscardAll<T>(List<IPlan<T>> plans)
        {
            foreach (var plan in plans)
            {
                Discard(plan);
            }
        }
        
        public void DiscardAll(List<IPlan> plans)
        {
            foreach (var plan in plans)
            {
                Discard(plan);
            }
        }

#endregion
    }
}

#endif