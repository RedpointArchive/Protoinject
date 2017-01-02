#if !PLATFORM_UNITY

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.Hosting;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
        
        public async Task<T> GetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes,
            IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = await PlanAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            await ValidateAsync(plan);
            return await ResolveAsync(plan);
        }

        public object Get(Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments,
            Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = Plan(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            Validate(plan);
            return Resolve(plan);
        }
        
        public async Task<object> GetAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes,
            IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = await PlanAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            await ValidateAsync(plan);
            return await ResolveAsync(plan);
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
        
        public async Task<T> TryGetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes,
            IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = await PlanAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            try
            {
                await ValidateAsync(plan);
                return await ResolveAsync(plan);
            }
            catch (Exception)
            {
                await DiscardAsync(plan);
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
        
        public async Task<object> TryGetAsync(Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plan = await PlanAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            try
            {
                await ValidateAsync(plan);
                return await ResolveAsync(plan);
            }
            catch (Exception)
            {
                await DiscardAsync(plan);
                return null;
            }
        }

        public T[] GetAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plans = PlanAll<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            ValidateAll(plans);
            return ResolveAll(plans);
        }
        
        public async Task<T[]> GetAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes,
            IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plans = await PlanAllAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            await ValidateAllAsync(plans);
            return await ResolveAllAsync(plans);
        }

        public object[] GetAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plans = PlanAll(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            ValidateAll(plans);
            return ResolveAll(plans);
        }
        
        public async Task<object[]> GetAllAsync(Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            var plans = await PlanAllAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
            await ValidateAllAsync(plans);
            return await ResolveAllAsync(plans);
        }

#endregion

#region Planning

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>) Plan(typeof (T), current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }
        
        public async Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes,
            IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>) await PlanAsync(typeof(T), current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public IPlan Plan(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return AsyncHelpers.RunSync(() => PlanAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings));
        }

        public async Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes,
            IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return await CreatePlan(type, current, bindingName, planName, null, injectionAttributes, arguments, transientBindings);
        }

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>)Plan(typeof(T), current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }
        
        public async Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, INode planRoot,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>) await PlanAsync(typeof(T), current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public IPlan Plan(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return AsyncHelpers.RunSync(() => PlanAsync(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings));
        }

        public async Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, INode planRoot,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return await CreatePlan(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }
        
        public IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>[])PlanAll(typeof(T), current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }
        
        public async Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes,
            IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>[]) await PlanAllAsync(typeof(T), current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }
        
        public IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return AsyncHelpers.RunSync(() => PlanAllAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings));
        }

        public async Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return await CreatePlans(type, current, bindingName, planName, null, injectionAttributes, arguments, transientBindings);
        }

        public IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>[])PlanAll(typeof(T), current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }
        
        public async Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, INode planRoot,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return (IPlan<T>[]) await PlanAllAsync(typeof(T), current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return AsyncHelpers.RunSync(() => PlanAllAsync(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings));
        }

        public async Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName, INode planRoot,
            IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return await CreatePlans(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

#endregion

#region Validation

        public void Validate<T>(IPlan<T> plan)
        {
            Validate((IPlan)plan);
        }
        
        public async Task ValidateAsync<T>(IPlan<T> plan)
        {
            await ValidateAsync((IPlan)plan);
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

        public void ValidateAll<T>(IPlan<T>[] plans)
        {
            for (var i = 0; i < plans.Length; i++)
            {
                Validate(plans[i]);
            }
        }
        
        public async Task ValidateAllAsync<T>(IPlan<T>[] plans)
        {
            var tasks = new Task[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                tasks[i] = ValidateAsync(plans[i]);
            }
            await Task.WhenAll(tasks);
        }

        public void ValidateAll(IPlan[] plans)
        {
            for (var i = 0; i < plans.Length; i++)
            {
                Validate(plans[i]);
            }
        }
        
        public async Task ValidateAllAsync(IPlan[] plans)
        {
            var tasks = new Task[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                tasks[i] = ValidateAsync(plans[i]);
            }
            await Task.WhenAll(tasks);
        }

#endregion

#region Resolution

        public INode<T> ResolveToNode<T>(IPlan<T> plan)
        {
            return (INode<T>)ResolveToNode((IPlan)plan);
        }
   
        public async Task<INode<T>> ResolveToNodeAsync<T>(IPlan<T> plan)
        {
            return (INode<T>) await ResolveToNodeAsync((IPlan)plan);
        }
    
        public INode<T>[] ResolveAllToNode<T>(IPlan<T>[] plans)
        {
            var results = new INode<T>[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                results[i] = ResolveToNode(plans[i]);
            }
            return results;
        }
        
        public async Task<INode<T>[]> ResolveAllToNodeAsync<T>(IPlan<T>[] plans)
        {
            var tasks = new Task<INode<T>>[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                tasks[i] = ResolveToNodeAsync(plans[i]);
            }
            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result).ToArray();
        }

        public INode[] ResolveAllToNode(IPlan[] plans)
        {
            var results = new INode[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                results[i] = ResolveToNode(plans[i]);
            }
            return results;
        }
        
        public async Task<INode[]> ResolveAllToNodeAsync(IPlan[] plans)
        {
            var tasks = new Task<INode>[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                tasks[i] = ResolveToNodeAsync(plans[i]);
            }
            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result).ToArray();
        }

        public T Resolve<T>(IPlan<T> plan)
        {
            return ResolveToNode(plan).Value;
        }
        
        public async Task<T> ResolveAsync<T>(IPlan<T> plan)
        {
            return (await ResolveToNodeAsync(plan)).Value;
        }

        public object Resolve(IPlan plan)
        {
            return ResolveToNode(plan).UntypedValue;
        }
        
        public async Task<object> ResolveAsync(IPlan plan)
        {
            return (await ResolveToNodeAsync(plan)).UntypedValue;
        }

        public T[] ResolveAll<T>(IPlan<T>[] plans)
        {
            var results = new T[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                results[i] = Resolve(plans[i]);
            }
            return results;
        }
        
        public async Task<T[]> ResolveAllAsync<T>(IPlan<T>[] plans)
        {
            var tasks = new Task<T>[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                tasks[i] = ResolveAsync(plans[i]);
            }
            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result).ToArray();
        }

        public object[] ResolveAll(IPlan[] plans)
        {
            var results = new object[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                results[i] = Resolve(plans[i]);
            }
            return results;
        }
        
        public async Task<object[]> ResolveAllAsync(IPlan[] plans)
        {
            var tasks = new Task<object>[plans.Length];
            for (var i = 0; i < plans.Length; i++)
            {
                tasks[i] = ResolveAsync(plans[i]);
            }
            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result).ToArray();
        }

#endregion

#region Discard

        public void Discard<T>(IPlan<T> plan)
        {
            Discard((IPlan)plan);
        }
        
        public async Task DiscardAsync<T>(IPlan<T> plan)
        {
            await DiscardAsync((IPlan)plan);
        }
        
        public void DiscardAll<T>(IPlan<T>[] plans)
        {
            foreach (var plan in plans)
            {
                Discard(plan);
            }
        }
        
        public async Task DiscardAllAsync<T>(IPlan<T>[] plans)
        {
            foreach (var plan in plans)
            {
                await DiscardAsync(plan);
            }
        }

        public void DiscardAll(IPlan[] plans)
        {
            foreach (var plan in plans)
            {
                Discard(plan);
            }
        }
        
        public async Task DiscardAllAsync(IPlan[] plans)
        {
            foreach (var plan in plans)
            {
                await DiscardAsync(plan);
            }
        }

#endregion
    }
}

#endif