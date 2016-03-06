using System;

namespace Protoinject
{
    public static class KernelExtensions
    {
        public static T Get<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(null, null, null, new IInjectionAttribute[0], arguments);
        }

        public static T Get<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(null, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static T Get<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(current, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static T Get<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.Get<T>(current, null, null, new IInjectionAttribute[0], arguments);
        }

        public static object Get(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, null, null, null, new IInjectionAttribute[0], arguments);
        }

        public static object Get(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, null, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static object Get(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, current, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static object Get(this IKernel kernel, Type type, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.Get(type, current, null, null, new IInjectionAttribute[0], arguments);
        }

        public static T TryGet<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(null, null, null, new IInjectionAttribute[0], arguments);
        }

        public static T TryGet<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(null, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static T TryGet<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(current, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static T TryGet<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet<T>(current, null, null, new IInjectionAttribute[0], arguments);
        }

        public static object TryGet(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, null, null, null, new IInjectionAttribute[0], arguments);
        }

        public static object TryGet(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, null, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static object TryGet(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, current, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static object TryGet(this IKernel kernel, Type type, INode current,
            params IConstructorArgument[] arguments)
        {
            return kernel.TryGet(type, current, null, null, new IInjectionAttribute[0], arguments);
        }
        
        public static T[] GetAll<T>(this IKernel kernel, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(null, null, null, new IInjectionAttribute[0], arguments);
        }

        public static T[] GetAll<T>(this IKernel kernel, string bindingName, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(null, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static T[] GetAll<T>(this IKernel kernel, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(current, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static T[] GetAll<T>(this IKernel kernel, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll<T>(current, null, null, new IInjectionAttribute[0], arguments);
        }

        public static object[] GetAll(this IKernel kernel, Type type, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, null, null, null, new IInjectionAttribute[0], arguments);
        }

        public static object[] GetAll(this IKernel kernel, Type type, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, null, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static object[] GetAll(this IKernel kernel, Type type, INode current, string bindingName,
            params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, current, bindingName, null, new IInjectionAttribute[0], arguments);
        }

        public static object[] GetAll(this IKernel kernel, Type type, INode current, params IConstructorArgument[] arguments)
        {
            return kernel.GetAll(type, current, null, null, new IInjectionAttribute[0], arguments);
        }
        
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
    }
}