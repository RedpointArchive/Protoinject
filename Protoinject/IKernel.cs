using System;
using System.Collections;
using System.Collections.Generic;
#if !PLATFORM_UNITY
using System.Threading.Tasks;
#endif

namespace Protoinject
{
    public interface IKernel
    {
        IHierarchy Hierarchy { get; }
        IDynamicResolutionFallback DynamicResolutionFallback { get; set; }

        IBindToInScopeWithDescendantFilterOrUniqueOrNamed<TInterface> Bind<TInterface>();
        IBindToInScopeWithDescendantFilterOrUniqueOrNamed Bind(Type @interface);
        void Unbind<T>();
        void Unbind(Type @interface);
        INode CreateEmptyNode(string name, INode parent = null);
        IScope CreateScopeFromNode(INode node);

        void Load<T>() where T : IProtoinjectModule;
        void Load(IProtoinjectModule module);
        
        T Get<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        object Get(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        T TryGet<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        object TryGet(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        T[] GetAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        object[] GetAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);

#if !PLATFORM_UNITY
        Task<T> GetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<object> GetAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<T> TryGetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<object> TryGetAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<T[]> GetAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<object[]> GetAllAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
#endif

        IPlan<T> Plan<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan Plan(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan<T> Plan<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan Plan(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
#if !PLATFORM_UNITY
        IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
#else
        List<IPlan<T>> PlanAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        List<IPlan> PlanAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        List<IPlan<T>> PlanAll<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        List<IPlan> PlanAll(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
#endif
        void Validate<T>(IPlan<T> plan);
        void Validate(IPlan plan);
#if !PLATFORM_UNITY
        void ValidateAll<T>(IPlan<T>[] plans);
        void ValidateAll(IPlan[] plans);
#else
        void ValidateAll<T>(List<IPlan<T>> plans);
        void ValidateAll(List<IPlan> plans);
#endif
        T Resolve<T>(IPlan<T> plan);
        object Resolve(IPlan plan);
#if !PLATFORM_UNITY
        T[] ResolveAll<T>(IPlan<T>[] plans);
        object[] ResolveAll(IPlan[] plans);
#else
        List<T> ResolveAll<T>(List<IPlan<T>> plans);
        List<object> ResolveAll(List<IPlan> plans);
#endif
        void Discard<T>(IPlan<T> plan);
        void Discard(IPlan plan);
#if !PLATFORM_UNITY
        void DiscardAll<T>(IPlan<T>[] plans);
        void DiscardAll(IPlan[] plans);
#else
        void DiscardAll<T>(List<IPlan<T>> plans);
        void DiscardAll(List<IPlan> plans);
#endif
        INode<T> ResolveToNode<T>(IPlan<T> plan);
        INode ResolveToNode(IPlan plan);
#if !PLATFORM_UNITY
        INode<T>[] ResolveAllToNode<T>(IPlan<T>[] plans);
        INode[] ResolveAllToNode(IPlan[] plans);
#else
        List<INode<T>> ResolveAllToNode<T>(List<IPlan<T>> plans);
        List<INode> ResolveAllToNode(List<IPlan> plans);
#endif

#if !PLATFORM_UNITY
        Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task ValidateAsync<T>(IPlan<T> plan);
        Task ValidateAsync(IPlan plan);
        Task ValidateAllAsync<T>(IPlan<T>[] plans);
        Task ValidateAllAsync(IPlan[] plans);
        Task<T> ResolveAsync<T>(IPlan<T> plan);
        Task<object> ResolveAsync(IPlan plan);
        Task<T[]> ResolveAllAsync<T>(IPlan<T>[] plans);
        Task<object[]> ResolveAllAsync(IPlan[] plans);
        Task DiscardAsync<T>(IPlan<T> plan);
        Task DiscardAsync(IPlan plan);
        Task DiscardAllAsync<T>(IPlan<T>[] plans);
        Task DiscardAllAsync(IPlan[] plans);
        Task<INode<T>> ResolveToNodeAsync<T>(IPlan<T> plan);
        Task<INode> ResolveToNodeAsync(IPlan plan);
        Task<INode<T>[]> ResolveAllToNodeAsync<T>(IPlan<T>[] plans);
        Task<INode[]> ResolveAllToNodeAsync(IPlan[] plans);
#endif
    }
}