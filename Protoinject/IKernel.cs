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

        void Load(IProtoinjectModule module);
        
        T Get<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments);
        object Get(Type type, INode current, string bindingName, string planName, params IConstructorArgument[] arguments);
        T TryGet<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments);
        object TryGet(Type type, INode current, string bindingName, string planName, params IConstructorArgument[] arguments);

        IPlan<T> Plan<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments);
        IPlan Plan(Type type, INode current, string bindingName, string planName, params IConstructorArgument[] arguments);
        void Validate<T>(IPlan<T> plan);
        void Validate(IPlan plan);
        T Resolve<T>(IPlan<T> plan);
        object Resolve(IPlan plan);
        void Discard<T>(IPlan<T> plan);
        void Discard(IPlan plan);
        INode<T> ResolveToNode<T>(IPlan<T> plan);
        INode ResolveToNode(IPlan plan);

        IEnumerable<T> GetAll<T>();
        IEnumerable GetAll(Type type);
    }
}