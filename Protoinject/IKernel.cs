using System;

namespace Protoinject
{
    public interface IKernel
    {
        //object Get(Type t, INode current = null, object[] additionalConstructorObjects = null);
        // T Get<T>(INode current = null);
        //IReadOnlyCollection<INode> GetRootHierarchies();
        IHierarchy Hierarchy { get; }
        IBindToInScopeWithDescendantFilterOrUnique<TInterface> Bind<TInterface>();
        INode CreateEmptyNode(string name, INode parent = null);
        IScope CreateScopeFromNode(INode node);


        IPlan<T> Plan<T>(INode current = null);
        IPlan Plan(Type t, INode current = null, object[] additionalConstructorObjects = null);
        void Validate<T>(IPlan<T> plan);
        void Validate(IPlan plan);
        T Resolve<T>(IPlan<T> plan);
        object Resolve(IPlan plan);
        INode<T> ResolveToNode<T>(IPlan<T> plan);
        INode ResolveToNode(IPlan plan);
    }
}