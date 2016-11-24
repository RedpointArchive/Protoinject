using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Protoinject
{
    public interface IKernel
    {
        IHierarchy Hierarchy { get; }
        IBindToInScopeWithDescendantFilterOrUniqueOrNamed<TInterface> Bind<TInterface>();
        IBindToInScopeWithDescendantFilterOrUniqueOrNamed Bind(Type @interface);
        void Unbind<T>();
        void Unbind(Type @interface);
        INode CreateEmptyNode(string name, INode parent = null);
        IScope CreateScopeFromNode(INode node);

        void Load<T>() where T : IProtoinjectModule;
        void Load(IProtoinjectModule module);
        
        T Get<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<T> GetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        object Get(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<object> GetAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        T TryGet<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<T> TryGetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        object TryGet(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<object> TryGetAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        T[] GetAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<T[]> GetAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        object[] GetAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<object[]> GetAllAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);

        IPlan<T> Plan<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan Plan(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan<T> Plan<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan Plan(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings);
        void Validate<T>(IPlan<T> plan);
        Task ValidateAsync<T>(IPlan<T> plan);
        void Validate(IPlan plan);
        Task ValidateAsync(IPlan plan);
        void ValidateAll<T>(IPlan<T>[] plans);
        Task ValidateAllAsync<T>(IPlan<T>[] plans);
        void ValidateAll(IPlan[] plans);
        Task ValidateAllAsync(IPlan[] plans);
        T Resolve<T>(IPlan<T> plan);
        Task<T> ResolveAsync<T>(IPlan<T> plan);
        object Resolve(IPlan plan);
        Task<object> ResolveAsync(IPlan plan);
        T[] ResolveAll<T>(IPlan<T>[] plans);
        Task<T[]> ResolveAllAsync<T>(IPlan<T>[] plans);
        object[] ResolveAll(IPlan[] plans);
        Task<object[]> ResolveAllAsync(IPlan[] plans);
        void Discard<T>(IPlan<T> plan);
        Task DiscardAsync<T>(IPlan<T> plan);
        void Discard(IPlan plan);
        Task DiscardAsync(IPlan plan);
        void DiscardAll<T>(IPlan<T>[] plans);
        Task DiscardAllAsync<T>(IPlan<T>[] plans);
        void DiscardAll(IPlan[] plans);
        Task DiscardAllAsync(IPlan[] plans);
        INode<T> ResolveToNode<T>(IPlan<T> plan);
        Task<INode<T>> ResolveToNodeAsync<T>(IPlan<T> plan);
        INode ResolveToNode(IPlan plan);
        Task<INode> ResolveToNodeAsync(IPlan plan);
        INode<T>[] ResolveAllToNode<T>(IPlan<T>[] plans);
        Task<INode<T>[]> ResolveAllToNodeAsync<T>(IPlan<T>[] plans);
        INode[] ResolveAllToNode(IPlan[] plans);
        Task<INode[]> ResolveAllToNodeAsync(IPlan[] plans);
    }
}