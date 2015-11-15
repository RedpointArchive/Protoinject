using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Protoinject
{
    public class StandardKernel : IKernel
    {
        private Dictionary<Type, List<IMapping>> _bindings;

        private IWritableHierarchy _hierarchy;
        private IScope _singletonScope;

        public StandardKernel()
        {
            _bindings = new Dictionary<Type, List<IMapping>>();
            _hierarchy = new DefaultHierarchy();
        }

        public IBindToInScopeWithDescendantFilterOrUniqueOrNamed<TInterface> Bind<TInterface>()
        {
            List<IMapping> list;
            if (!_bindings.ContainsKey(typeof (TInterface)))
            {
                list = new List<IMapping>();
                _bindings[typeof (TInterface)] = list;
            }
            else
            {
                list = _bindings[typeof (TInterface)];
            }
            var mapping = new DefaultMapping();
            mapping.Target = typeof (TInterface);
            list.Add(mapping);
            return new DefaultBindToInScopeWithDescendantFilterOrUnique<TInterface>(this, mapping);
        }

        public IBindToInScopeWithDescendantFilterOrUniqueOrNamed Bind(Type @interface)
        {
            List<IMapping> list;
            if (!_bindings.ContainsKey(@interface))
            {
                list = new List<IMapping>();
                _bindings[@interface] = list;
            }
            else
            {
                list = _bindings[@interface];
            }
            var mapping = new DefaultMapping();
            mapping.Target = @interface;
            list.Add(mapping);
            return new DefaultBindToInScopeWithDescendantFilterOrUniqueOrNamed(this, mapping);
        }

        public void Unbind<T>()
        {
            throw new NotImplementedException();
        }

        public void Unbind(Type @interface)
        {
            throw new NotImplementedException();
        }

        public IHierarchy Hierarchy => _hierarchy;

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            return (IPlan<T>) Plan(typeof (T), current, bindingName, planName, arguments);
        }

        public IPlan Plan(Type t, INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            return CreatePlan(t, current, bindingName, planName, null, arguments);
        }

        public void Validate<T>(IPlan<T> plan)
        {
            Validate((IPlan)plan);
        }

        public void Validate(IPlan plan)
        {
            if (!plan.Valid)
            {
                throw new ActivationException("The planned node is not valid (hint: " + plan.InvalidHint + ")", plan);
            }

            foreach (var toCreate in plan.PlannedCreatedNodes)
            {
                if (!toCreate.Valid)
                {
                    throw new ActivationException("The planned node is not valid (hint: " + toCreate.InvalidHint + ")", plan);
                }
            }

            // TODO: Validate more configuration
        }

        public T Resolve<T>(IPlan<T> plan)
        {
            return ResolveToNode(plan).Value;
        }

        public object Resolve(IPlan plan)
        {
            return ResolveToNode(plan).UntypedValue;
        }

        public void Discard<T>(IPlan<T> plan)
        {
            Discard((IPlan)plan);
        }

        public void Discard(IPlan plan)
        {
            var planAsNode = (DefaultNode) plan;
            planAsNode.Discarded = true;
            foreach (var plan1 in planAsNode.PlannedCreatedNodes)
            {
                var toCreate = (DefaultNode) plan1;
                var parent = toCreate.ParentPlan;
                if (parent != null)
                {
                    ((DefaultNode) parent)?.ChildrenInternal.Remove((INode) toCreate);
                }
                else
                {
                    _hierarchy.RootNodes.Remove(toCreate);
                }
                toCreate.Parent = null;
                toCreate.Discarded = true;
            }
            var nodeParent = planAsNode.ParentPlan;
            if (nodeParent != null)
            {
                ((DefaultNode)nodeParent)?.ChildrenInternal.Remove(planAsNode);
            }
            else
            {
                _hierarchy.RootNodes.Remove(planAsNode);
            }
        }

        public INode<T> ResolveToNode<T>(IPlan<T> plan)
        {
            return (INode<T>) ResolveToNode((IPlan) plan);
        }

        public INode ResolveToNode(IPlan plan)
        {
            if (!plan.Planned)
            {
                return (INode) plan;
            }

            foreach (var dependant in plan.DependentOnPlans)
            {
                if (dependant.Discarded)
                {
                    throw new ActivationException("This plan was dependant on plan '" + dependant.FullName + "' / '" +
                                                  dependant.PlanName +
                                                  "', but that plan has since been discarded.  Re-create this plan to resolve it.",
                        plan);
                }
                else if (dependant.Planned)
                {
                    throw new ActivationException("This plan is dependant on plan '" + dependant.FullName + "' / '" +
                                                  dependant.PlanName + "', but that plan is not resolved yet.", plan);
                }
            }

            foreach (var node in plan.PlannedCreatedNodes)
            {
                var toCreate = (DefaultNode) node;
                if (toCreate.Planned && toCreate.UntypedValue != null)
                {
                    // This is a factory.
                    toCreate.Planned = false;
                }
                else if (toCreate.Planned)
                {
                    var parameters = new List<object>();
                    foreach (var argument in toCreate.PlannedConstructorArguments)
                    {
                        parameters.Add(ResolveArgument(toCreate, argument));
                    }
                    toCreate.UntypedValue = toCreate.PlannedConstructor.Invoke(parameters.ToArray());
                    toCreate.Planned = false;
                }
            }

            return (INode) plan;
        }

        public IEnumerable<T> GetAll<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable GetAll(Type type)
        {
            throw new NotImplementedException();
        }

        private object ResolveArgument(DefaultNode toCreate, IUnresolvedArgument argument)
        {
            switch (argument.ArgumentType)
            {
                case UnresolvedArgumentType.Type:
                    if (argument.PlannedTarget.Planned)
                    {
                        throw new ActivationException(
                            "Expected " + argument.PlannedTarget.FullName + " to be resolved by now.", toCreate);
                    }
                    return ((DefaultNode) argument.PlannedTarget).UntypedValue;
                case UnresolvedArgumentType.Factory:
                    return argument.FactoryDelegate;
                case UnresolvedArgumentType.FactoryArgument:
                    return argument.FactoryArgumentValue;
                case UnresolvedArgumentType.CurrentNode:
                    return argument.CurrentNode;
                case UnresolvedArgumentType.KnownValue:
                    return argument.KnownValue;
            }

            throw new ActivationException("Unexpected argument type", toCreate);
        }

        private IPlan CreatePlan(Type requestedType, INode current, string bindingName, string planName, INode planRoot, IConstructorArgument[] arguments)
        {
            var resolvedMapping = ResolveType(requestedType, bindingName, current);

            var scopeNode = current;
            if (resolvedMapping.LifetimeScope != null)
            {
                scopeNode = resolvedMapping.LifetimeScope.GetContainingNode();
            }

            if (scopeNode != null && resolvedMapping.UniquePerScope)
            {
                var existing = scopeNode.Children.FirstOrDefault(x => x.Type.IsAssignableFrom(resolvedMapping.Target));
                if (existing != null)
                {
                    if (existing.Planned)
                    {
                        // Flag that the plan root is now dependant on the other
                        // plan being resolved.
                        planRoot?.DependentOnPlans.Add(existing.PlanRoot);
                    }

                    return existing;
                }
            }

            Type nodeToCreate;
            if (resolvedMapping.Target != null)
            {
                nodeToCreate = typeof(DefaultNode<>).MakeGenericType(resolvedMapping.Target);
            }
            else
            {
                nodeToCreate = typeof(DefaultNode<>).MakeGenericType(requestedType);
            }
            var createdNode = (DefaultNode) Activator.CreateInstance(nodeToCreate);
            createdNode.Name = string.Empty;
            createdNode.Parent = scopeNode;
            createdNode.Planned = true;
            createdNode.Type = resolvedMapping.Target ?? requestedType;
            createdNode.PlanName = planName;
            createdNode.PlanRoot = planRoot;

            // If there is no plan root, then we are the plan root.
            if (planRoot == null)
            {
                planRoot = createdNode;
            }

            try
            {
                // TODO: Handle ToMethod mappings.

                if (resolvedMapping.TargetFactory)
                {
                    var attribute = createdNode.Type.GetCustomAttribute<GeneratedFactoryAttribute>();
                    var resolvedFactoryClass = createdNode.Type.Assembly.GetTypes().FirstOrDefault(x => x.FullName == attribute.FullTypeName);
                    if (resolvedFactoryClass == null)
                    {
                        // This node won't be valid because it's planned, has no value and
                        // has no constructor.
                        createdNode.InvalidHint = "The generated factory class '" + attribute.FullTypeName +
                                                  "' could not be found in the assembly.";
                        return createdNode;
                    }
                    createdNode.Type = resolvedFactoryClass;
                }

                if (createdNode.Type == null)
                {
                    // This node won't be valid because it's planned, has no value and
                    // has no constructor.
                    createdNode.InvalidHint = "There was no valid target for the binding (is the 'To' method missing?)";
                    return createdNode;
                }

                createdNode.PlannedConstructor =
                    createdNode.Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
                if (createdNode.PlannedConstructor == null)
                {
                    // This node won't be valid because it's planned, has no value and
                    // has no constructor.
                    createdNode.InvalidHint = "There was no valid public constructor for '" + createdNode.Type.FullName + "'";
                    return createdNode;
                }

                createdNode.PlannedConstructorArguments = new List<IUnresolvedArgument>();

                var parameters = createdNode.PlannedConstructor.GetParameters();

                var slots = new DefaultUnresolvedArgument[parameters.Length];

                // First apply additional constructor arguments to the slots.
                if (arguments != null)
                {
                    foreach (var additional in arguments)
                    {
                        for (var s = 0; s < slots.Length; s++)
                        {
                            if (additional.Satisifies(createdNode.PlannedConstructor, parameters[s]))
                            {
                                var plannedArgument = new DefaultUnresolvedArgument();
                                plannedArgument.ArgumentType = UnresolvedArgumentType.FactoryArgument;
                                plannedArgument.FactoryArgumentValue = additional.GetValue();
                                slots[s] = plannedArgument;
                            }
                        }
                    }
                }

                for (var i = 0; i < slots.Length; i++)
                {
                    if (slots[i] != null)
                    {
                        // Already filled in.
                        continue;
                    }

                    var parameter = parameters[i];

                    var plannedArgument = new DefaultUnresolvedArgument();
                    plannedArgument.ParameterName = parameter.Name;

                    if (parameter.ParameterType == typeof (ICurrentNode))
                    {
                        plannedArgument.ArgumentType = UnresolvedArgumentType.CurrentNode;
                        plannedArgument.CurrentNode = new DefaultCurrentNode(createdNode);
                    }
                    else if (parameter.ParameterType == typeof(IKernel))
                    {
                        plannedArgument.ArgumentType = UnresolvedArgumentType.KnownValue;
                        plannedArgument.KnownValue = this;
                    }
                    else
                    {
                        plannedArgument.ArgumentType = UnresolvedArgumentType.Type;
                        plannedArgument.UnresolvedType = parameters[i].ParameterType;
                    }

                    slots[i] = plannedArgument;
                }

                createdNode.PlannedConstructorArguments = new List<IUnresolvedArgument>(slots);

                foreach (var argument in createdNode.PlannedConstructorArguments)
                {
                    switch (argument.ArgumentType)
                    {
                        case UnresolvedArgumentType.Type:
                            var child = CreatePlan(
                                argument.UnresolvedType,
                                createdNode, 
                                null,
                                planName, 
                                planRoot,
                                null);
                            if (child.ParentPlan == createdNode)
                            {
                                createdNode.ChildrenInternal.Add((INode) child);
                            }
                            ((DefaultUnresolvedArgument) argument).PlannedTarget = child;
                            break;
                    }
                }

                if (createdNode.Parent == null)
                {
                    _hierarchy.RootNodes.Add(createdNode);
                }
                else //if (scopeNode != current)
                {
                    ((DefaultNode) scopeNode).ChildrenInternal.Add(createdNode);
                }

                return createdNode;
            }
            finally
            {
                planRoot.PlannedCreatedNodes.Add(createdNode);
            }
        }

        private IMapping ResolveType(Type originalType, string name, INode current)
        {
            // Try to resolve the type using bindings first.
            if (_bindings.ContainsKey(originalType))
            {
                var bindings = _bindings[originalType];
                var sortedBindings = bindings.Where(x => x.Named == name)
                    .OrderBy(x => x.OnlyUnderDescendantFilter != null ? 0 : 1);
                foreach (var b in sortedBindings)
                {
                    if (b.OnlyUnderDescendantFilter != null)
                    {
                        var parents = b.OnlyUnderDescendantFilter.GetParents();
                        if (!parents.Contains(current))
                        {
                            continue;
                        }
                    }

                    return b;
                }
            }

            // If the type is a concrete type, return it.
            if (!originalType.IsAbstract && !originalType.IsInterface)
            {
                return new DefaultMapping
                {
                    Target = originalType
                };
            }

            // We can't resolve this type.
            return new InvalidMapping();
        }

        public IScope CreateScopeFromNode(INode node)
        {
            return new FixedScope(node);
        }

        public void Load<T>() where T : IProtoinjectModule
        {
            Activator.CreateInstance<T>().Load(this);
        }

        public void Load(IProtoinjectModule module)
        {
            module.Load(this);
        }

        public T Get<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            var plan = Plan<T>(current, bindingName, planName, arguments);
            Validate(plan);
            return Resolve(plan);
        }

        public object Get(Type type, INode current, string bindingName, string planName,
            params IConstructorArgument[] arguments)
        {
            var plan = Plan(type, current, bindingName, planName, arguments);
            Validate(plan);
            return Resolve(plan);
        }

        public T TryGet<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            var plan = Plan<T>(current, bindingName, planName, arguments);
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
            params IConstructorArgument[] arguments)
        {
            var plan = Plan(type, current, bindingName, planName, arguments);
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

        public INode CreateEmptyNode(string name, INode parent = null)
        {
            var node = new DefaultNode
            {
                Parent = parent,
                Name = DefaultNode.NormalizeName(name)
            };
            if (parent == null)
            {
                _hierarchy.RootNodes.Add(node);
            }
            else
            {
                ((DefaultNode) parent).ChildrenInternal.Add(node);
            }
            return node;
        }

        public IScope GetSingletonScope()
        {
            if (_singletonScope == null)
            {
                _singletonScope = CreateScopeFromNode(CreateEmptyNode("Singletons"));
            }

            return _singletonScope;
        }
    }
}