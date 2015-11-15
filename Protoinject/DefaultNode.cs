using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protoinject
{
    internal class DefaultNode<T> : DefaultNode, INode<T>
    {
        public T Value { get { return (T)UntypedValue; } }
    }

    internal class DefaultNode : INode
    {
        public DefaultNode()
        {
            ChildrenInternal = new List<INode>();
            PlannedCreatedNodes = new List<IPlan>();
            DependentOnPlans = new List<IPlan>();
        }

        internal List<INode> ChildrenInternal { get; }

        public INode Parent { get; set; }
        public string Name { get; set; }

        public IReadOnlyCollection<INode> Children => ChildrenInternal.AsReadOnly();

        public List<IPlan> PlannedCreatedNodes { get; }

        public string FullName
        {
            get
            {
                return
                    GetParents()
                        .Concat(new[] {this})
                        .Select(x => string.IsNullOrWhiteSpace(x.Name) ? (x.Type == null ? "(unknown)" : ("(" + x.Type.FullName + ")")) : x.Name)
                        .Aggregate((a, b) => a + "/" + b);
            }
        }

        public IPlan PlanRoot { get; set; }
        public List<IPlan> DependentOnPlans { get; }
        public bool Discarded { get; set; }

        public bool Valid
        {
            get
            {
                if (this.Planned)
                {
                    return UntypedValue != null || PlannedConstructor != null || PlannedMethod != null;
                }

                return !Discarded;
            }
        }

        public object UntypedValue { get; set; }

        public Type Type { get; set; }

        public IReadOnlyCollection<INode> GetParents()
        {
            var parents = new List<INode>();
            var current = Parent;
            while (current != null)
            {
                parents.Insert(0, current);
                current = current.Parent;
            }
            return parents.AsReadOnly();
        }

        public bool Planned { get; set; }
        public string PlanName { get; set; }

        public void SetTypeAndValue(Type type, object value)
        {
            Type = type;
            UntypedValue = value;
        }

        public IPlan ParentPlan
        {
            get { return Parent; }
        }

        public IReadOnlyCollection<IPlan> ChildrenPlan => Children;
        public ConstructorInfo PlannedConstructor { get; set; }

        public List<IUnresolvedArgument> PlannedConstructorArguments { get; set; }
        public string InvalidHint { get; set; }
        public Func<IContext, object> PlannedMethod { get; set; }

        public static string NormalizeName(string name)
        {
            return (name ?? string.Empty).Replace("/", "");
        }

        public override string ToString()
        {
            return this.GetDebugRepresentation();
        }
    }
}