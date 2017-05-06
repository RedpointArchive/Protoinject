using System;
using System.Collections.Generic;
#if !PLATFORM_UNITY
using System.Threading.Tasks;
#endif

namespace Protoinject
{
    public static class KernelExtensions
    {
        #region Compatibility for new Transient Bindings parameter

        public static T Get<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static object Get(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static T TryGet<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static object TryGet(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static T[] GetAll<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static object[] GetAll(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static IPlan<T> Plan<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.Plan<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static IPlan Plan(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.Plan(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static T Get<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static object Get(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static T TryGet<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static object TryGet(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static T[] GetAll<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static object[] GetAll(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static IPlan<T> Plan<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.Plan<T>(current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static IPlan Plan(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.Plan(type, current, bindingName, planName, injectionAttributes, arguments, null);
        }

#if !PLATFORM_UNITY
        public static IPlan<T>[] PlanAll<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static IPlan[] PlanAll(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static IPlan<T>[] PlanAll<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll<T>(current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static IPlan[] PlanAll(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll(type, current, bindingName, planName, injectionAttributes, arguments, null);
        }
#else
        public static List<IPlan<T>> PlanAll<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static List<IPlan> PlanAll(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, Dictionary<Type, List<IMapping>> transientBindings,
            params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public static List<IPlan<T>> PlanAll<T>(this IKernel kernel, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll<T>(current, bindingName, planName, injectionAttributes, arguments, null);
        }

        public static List<IPlan> PlanAll(this IKernel kernel, Type type, INode current, string bindingName, string planName,
            IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments)
        {
            return kernel.PlanAll(type, current, bindingName, planName, injectionAttributes, arguments, null);
        }
#endif

        #endregion

        #region Easy-of-use methods

        public static T Get<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T Get<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T Get<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T Get<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object Get(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object Get(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object Get(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object Get(this IKernel kernel, Type type, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T TryGet<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T TryGet<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T TryGet<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T TryGet<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object TryGet(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object TryGet(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object TryGet(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object TryGet(this IKernel kernel, Type type, INode current,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, current, null, null, new IInjectionAttribute[0], arguments, null);
        }
        
        public static T[] GetAll<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T[] GetAll<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T[] GetAll<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static T[] GetAll<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object[] GetAll(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object[] GetAll(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object[] GetAll(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static object[] GetAll(this IKernel kernel, Type type, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, current, null, null, new IInjectionAttribute[0], arguments, null);
        }

#if !PLATFORM_UNITY

        public static Task<T> GetAsync<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync<T>(null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T> GetAsync<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync<T>(null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T> GetAsync<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync<T>(current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T> GetAsync<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync<T>(current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> GetAsync(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync(type, null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> GetAsync(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync(type, null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> GetAsync(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync(type, current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> GetAsync(this IKernel kernel, Type type, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAsync(type, current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T> TryGetAsync<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync<T>(null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T> TryGetAsync<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync<T>(null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T> TryGetAsync<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync<T>(current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T> TryGetAsync<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync<T>(current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> TryGetAsync(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync(type, null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> TryGetAsync(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync(type, null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> TryGetAsync(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync(type, current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object> TryGetAsync(this IKernel kernel, Type type, INode current,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGetAsync(type, current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T[]> GetAllAsync<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync<T>(null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T[]> GetAllAsync<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync<T>(null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T[]> GetAllAsync<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync<T>(current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<T[]> GetAllAsync<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync<T>(current, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object[]> GetAllAsync(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync(type, null, null, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object[]> GetAllAsync(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync(type, null, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object[]> GetAllAsync(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync(type, current, bindingName, null, new IInjectionAttribute[0], arguments, null);
        }

        public static Task<object[]> GetAllAsync(this IKernel kernel, Type type, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAllAsync(type, current, null, null, new IInjectionAttribute[0], arguments, null);
        }

#endif

        public static IBindToInScopeWithDescendantFilterOrUniqueOrNamed<T> Rebind<T>(this IKernel kernel)
        {
            kernel.Unbind<T>();
            return kernel.Bind<T>();
        }

        public static IBindToInScopeWithDescendantFilterOrUniqueOrNamed Rebind(this IKernel kernel, Type @interface)
        {
            kernel.Unbind(@interface);
            return kernel.Bind(@interface);
        }

#endregion
    }
}