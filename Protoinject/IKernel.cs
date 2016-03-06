using System;
using System.Collections;
using System.Collections.Generic;

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
        
        T Get<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        object Get(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        T TryGet<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        object TryGet(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        T[] GetAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        object[] GetAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);

        IPlan<T> Plan<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        IPlan Plan(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, params IConstructorArgument[] arguments);
        void Validate<T>(IPlan<T> plan);
        void Validate(IPlan plan);
        void ValidateAll<T>(IPlan<T>[] plans);
        void ValidateAll(IPlan[] plans);
        T Resolve<T>(IPlan<T> plan);
        object Resolve(IPlan plan);
        T[] ResolveAll<T>(IPlan<T>[] plans);
        object[] ResolveAll(IPlan[] plans);
        void Discard<T>(IPlan<T> plan);
        void Discard(IPlan plan);
        void DiscardAll<T>(IPlan<T>[] plans);
        void DiscardAll(IPlan[] plans);
        INode<T> ResolveToNode<T>(IPlan<T> plan);
        INode ResolveToNode(IPlan plan);
        INode<T>[] ResolveAllToNode<T>(IPlan<T>[] plans);
        INode[] ResolveAllToNode(IPlan[] plans);
    }
}