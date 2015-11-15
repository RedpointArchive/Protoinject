using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Hosting;

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

        public IHierarchy Hierarchy => _hierarchy;

        #region Module Loading

        public void Load<T>() where T : IProtoinjectModule
        {
            Activator.CreateInstance<T>().Load(this);
        }

        public void Load(IProtoinjectModule module)
        {
            module.Load(this);
        }

        #endregion

        #region Binding / Unbinding

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
            _bindings[typeof(T)] = new List<IMapping>();
        }

        public void Unbind(Type @interface)
        {
            _bindings[@interface] = new List<IMapping>();
        }

        #endregion

        #region Scope / Node Control

        public IScope CreateScopeFromNode(INode node)
        {
            return new FixedScope(node);
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
                var childrenInternal = ((DefaultNode)parent).ChildrenInternal;
                if (!childrenInternal.Contains(node))
                {
                    childrenInternal.Add(node);
                }
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

        #endregion

        #region Get / TryGet / GetAll

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

        public T[] GetAll<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            var plans = PlanAll<T>(current, bindingName, planName, arguments);
            ValidateAll(plans);
            return ResolveAll(plans);
        }

        public object[] GetAll(Type type, INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            var plans = PlanAll(type, current, bindingName, planName, arguments);
            ValidateAll(plans);
            return ResolveAll(plans);
        }

        #endregion

        #region Planning

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            return (IPlan<T>) Plan(typeof (T), current, bindingName, planName, arguments);
        }

        public IPlan Plan(Type type, INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            return CreatePlan(type, current, bindingName, planName, null, arguments);
        }

        public IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            return (IPlan<T>[])PlanAll(typeof(T), current, bindingName, planName, arguments);
        }

        public IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, params IConstructorArgument[] arguments)
        {
            return CreatePlans(type, current, bindingName, planName, null, arguments);
        }

        #endregion

        #region Validation

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

                var node = (DefaultNode)toCreate;
                if (node.Planned && node.UntypedValue != null)
                {
                }
                else if (node.Planned && node.PlannedMethod != null)
                {
                }
                else if (node.Planned)
                {
                    foreach (var argument in node.PlannedConstructorArguments)
                    {
                        switch (argument.ArgumentType)
                        {
                            case UnresolvedArgumentType.Type:
                                if (argument.IsMultipleResult)
                                {
                                    for (int index = 0; index < argument.PlannedTargets.Length; index++)
                                    {
                                        var target = argument.PlannedTargets[index];

                                        if (!argument.IsOptional && !target.Valid)
                                        {
                                            throw new ActivationException("The planned node is not valid (hint: " + target.InvalidHint + ")", target);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!argument.IsOptional && !argument.PlannedTarget.Valid)
                                    {
                                        throw new ActivationException("The planned node is not valid (hint: " + argument.PlannedTarget.InvalidHint + ")", argument.PlannedTarget);
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            // TODO: Validate more configuration
        }

        public void ValidateAll<T>(IPlan<T>[] plans)
        {
            foreach (var plan in plans)
            {
                Validate(plan);
            }
        }

        public void ValidateAll(IPlan[] plans)
        {
            foreach (var plan in plans)
            {
                Validate(plan);
            }
        }

        #endregion

        #region Resolution

        public INode<T> ResolveToNode<T>(IPlan<T> plan)
        {
            return (INode<T>)ResolveToNode((IPlan)plan);
        }

        public INode ResolveToNode(IPlan plan)
        {
            if (!plan.Planned)
            {
                return (INode)plan;
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
                var toCreate = (DefaultNode)node;
                if (toCreate.Planned && toCreate.UntypedValue != null)
                {
                    // This is a factory.
                    toCreate.Planned = false;
                }
                else if (toCreate.Planned && toCreate.PlannedMethod != null)
                {
                    toCreate.Planned = false;
                    toCreate.UntypedValue = toCreate.PlannedMethod(new DefaultContext(this, ((DefaultNode)node).Parent, node));
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

            return (INode)plan;
        }

        public INode<T>[] ResolveAllToNode<T>(IPlan<T>[] plans)
        {
            return plans.Select(ResolveToNode).ToArray();
        }

        public INode[] ResolveAllToNode(IPlan[] plans)
        {
            return plans.Select(ResolveToNode).ToArray();
        }

        public T Resolve<T>(IPlan<T> plan)
        {
            return ResolveToNode(plan).Value;
        }

        public object Resolve(IPlan plan)
        {
            return ResolveToNode(plan).UntypedValue;
        }

        public T[] ResolveAll<T>(IPlan<T>[] plans)
        {
            return plans.Select(Resolve).ToArray();
        }

        public object[] ResolveAll(IPlan[] plans)
        {
            return plans.Select(Resolve).ToArray();
        }

        #endregion

        #region Discard

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

        public void DiscardAll<T>(IPlan<T>[] plans)
        {
            foreach (var plan in plans)
            {
                Discard(plan);
            }
        }

        public void DiscardAll(IPlan[] plans)
        {
            foreach (var plan in plans)
            {
                Discard(plan);
            }
        }

        #endregion

        #region Internals

        private object ResolveArgument(DefaultNode toCreate, IUnresolvedArgument argument)
        {
            switch (argument.ArgumentType)
            {
                case UnresolvedArgumentType.Type:
                    if (argument.IsMultipleResult)
                    {
                        var value = (Array)Activator.CreateInstance(argument.MultipleResultElementType.MakeArrayType(), new object[] { argument.PlannedTargets.Length });
                        for (int index = 0; index < argument.PlannedTargets.Length; index++)
                        {
                            var target = argument.PlannedTargets[index];

                            if (target.Planned)
                            {
                                throw new ActivationException(
                                    "Expected " + target.FullName + " to be resolved by now.", toCreate);
                            }

                            value.SetValue(((DefaultNode)target).UntypedValue, index);
                        }
                        return value;
                    }
                    else
                    {
                        if (argument.PlannedTarget.Planned)
                        {
                            if (argument.IsOptional)
                            {
                                return null;
                            }
                            else
                            {
                                throw new ActivationException(
                                    "Expected " + argument.PlannedTarget.FullName + " to be resolved by now.", toCreate);
                            }
                        }
                        return ((DefaultNode)argument.PlannedTarget).UntypedValue;
                    }
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

        private IPlan CreatePlan(Type requestedType, INode current, string bindingName, string planName,
            INode planRoot, IConstructorArgument[] arguments)
        {
            var plans = CreatePlans(requestedType, current, bindingName, planName, planRoot, arguments);
            if (plans.Length != 1)
            {
                foreach (var plan in plans)
                {
                    Discard(plan);
                }

                var nodeToCreate = typeof(DefaultNode<>).MakeGenericType(requestedType);
                var createdNode = (DefaultNode)Activator.CreateInstance(nodeToCreate);
                createdNode.Parent = current;
                createdNode.Planned = true;

                if (plans.Length == 0)
                {
                    createdNode.InvalidHint = "Expected one binding for '" + requestedType +
                                              "' but no types were bound.";
                }
                else
                {
                    createdNode.InvalidHint = "Expected only one binding for '" + requestedType +
                                              "' but multiple types were bound.";
                }

                return createdNode;
            }

            return plans[0];
        }

        private IPlan[] CreatePlans(Type requestedType, INode current, string bindingName, string planName, INode planRoot, IConstructorArgument[] arguments)
        {
            var resolvedMappings = ResolveTypes(requestedType, bindingName, current);
            var plans = (IPlan[])Activator.CreateInstance(typeof(IPlan<>).MakeGenericType(requestedType).MakeArrayType(), resolvedMappings.Length);

            for (var i = 0; i < resolvedMappings.Length; i++)
            {
                var resolvedMapping = resolvedMappings[i];

                // If the resolved target is a generic type definition, we need to fill in the
                // generic type arguments from the request.
                var targetNonGeneric = resolvedMapping.Target;
                if (targetNonGeneric != null && targetNonGeneric.IsGenericTypeDefinition)
                {
                    targetNonGeneric = targetNonGeneric.MakeGenericType(requestedType.GenericTypeArguments);
                }

                var scopeNode = current;
                if (resolvedMapping.LifetimeScope != null)
                {
                    scopeNode = resolvedMapping.LifetimeScope.GetContainingNode();
                }

                if (scopeNode != null && resolvedMapping.UniquePerScope)
                {
                    var existing =
                        scopeNode.Children.FirstOrDefault(x => x.Type.IsAssignableFrom(targetNonGeneric));
                    if (existing != null)
                    {
                        if (existing.Planned && existing.PlanRoot != planRoot)
                        {
                            // Flag that the plan root is now dependant on the other
                            // plan being resolved.
                            planRoot?.DependentOnPlans.Add(existing.PlanRoot);
                        }

                        plans[i] = existing;
                        continue;
                    }
                }

                Type nodeToCreate;
                if (targetNonGeneric != null)
                {
                    nodeToCreate = typeof (DefaultNode<>).MakeGenericType(targetNonGeneric);
                }
                else
                {
                    nodeToCreate = typeof (DefaultNode<>).MakeGenericType(requestedType);
                }
                var createdNode = (DefaultNode) Activator.CreateInstance(nodeToCreate);
                createdNode.Name = string.Empty;
                createdNode.Parent = scopeNode;
                createdNode.Planned = true;
                createdNode.Type = targetNonGeneric ?? requestedType;
                createdNode.PlanName = planName;
                createdNode.PlanRoot = planRoot;

                if (createdNode.Type.ContainsGenericParameters)
                {
                    throw new InvalidOperationException("The type still contained generic type parameters even after initial binding resolution.");
                }

                // If there is no plan root, then we are the plan root.
                if (planRoot == null)
                {
                    planRoot = createdNode;
                }

                try
                {
                    if (resolvedMapping.TargetMethod != null)
                    {
                        createdNode.PlannedMethod = resolvedMapping.TargetMethod;
                        plans[i] = createdNode;
                        continue;
                    }

                    if (resolvedMapping.TargetFactory)
                    {
                        var attribute = createdNode.Type.GetCustomAttribute<GeneratedFactoryAttribute>();
                        if (attribute == null)
                        {
                            // This node won't be valid because it's planned, has no value and
                            // has no constructor.
                            createdNode.InvalidHint = "The factory interface '" + createdNode.Type +
                                                      "' doesn't have a generated factory for it.  " +
                                                      "Make sure the factory interface inherits from " +
                                                      "IGenerateFactory so that the generator will " +
                                                      "implement it for you.";
                            plans[i] = createdNode;
                            continue;
                        }

                        var resolvedFactoryClass =
                            createdNode.Type.Assembly.GetTypes()
                                .FirstOrDefault(x => x.FullName == attribute.FullTypeName);
                        if (resolvedFactoryClass == null)
                        {
                            // This node won't be valid because it's planned, has no value and
                            // has no constructor.
                            createdNode.InvalidHint = "The generated factory class '" + attribute.FullTypeName +
                                                      "' could not be found in the assembly.";
                            plans[i] = createdNode;
                            continue;
                        }

                        // If the factory class is generic, pass in type parameters as needed.
                        if (resolvedFactoryClass != null && resolvedFactoryClass.IsGenericTypeDefinition)
                        {
                            resolvedFactoryClass = resolvedFactoryClass.MakeGenericType(requestedType.GenericTypeArguments);
                        }

                        createdNode.Type = resolvedFactoryClass;
                    }

                    if (createdNode.Type == null)
                    {
                        // This node won't be valid because it's planned, has no value and
                        // has no constructor.
                        createdNode.InvalidHint =
                            "There was no valid target for the binding (is the 'To' method missing?)";
                        plans[i] = createdNode;
                        continue;
                    }

                    if (createdNode.Type == requestedType && (requestedType.IsInterface || requestedType.IsAbstract))
                    {
                        // This node won't be valid because it's planned, has no value and
                        // has no constructor.
                        createdNode.InvalidHint =
                            "The target type '" + requestedType + "' isn't valid because it can't be constructed.";
                        plans[i] = createdNode;
                        continue;
                    }

                    createdNode.PlannedConstructor =
                        createdNode.Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
                    if (createdNode.PlannedConstructor == null)
                    {
                        // This node won't be valid because it's planned, has no value and
                        // has no constructor.
                        createdNode.InvalidHint = "There was no valid public constructor for '" +
                                                  createdNode.Type.FullName + "'";
                        plans[i] = createdNode;
                        continue;
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

                    for (var ii = 0; ii < slots.Length; ii++)
                    {
                        if (slots[ii] != null)
                        {
                            // Already filled in.
                            continue;
                        }

                        var parameter = parameters[ii];

                        var plannedArgument = new DefaultUnresolvedArgument();
                        plannedArgument.ParameterName = parameter.Name;

                        if (parameter.ParameterType == typeof (ICurrentNode))
                        {
                            plannedArgument.ArgumentType = UnresolvedArgumentType.CurrentNode;
                            plannedArgument.CurrentNode = new DefaultCurrentNode(createdNode);
                        }
                        else if (parameter.ParameterType == typeof (IKernel))
                        {
                            plannedArgument.ArgumentType = UnresolvedArgumentType.KnownValue;
                            plannedArgument.KnownValue = this;
                        }
                        else
                        {
                            plannedArgument.ArgumentType = UnresolvedArgumentType.Type;
                            plannedArgument.UnresolvedType = parameters[ii].ParameterType;
                            plannedArgument.ParameterName = parameters[ii].GetCustomAttribute<NamedAttribute>()?.Name;
                            plannedArgument.IsOptional = parameters[ii].GetCustomAttribute<OptionalAttribute>() != null;
                        }

                        slots[ii] = plannedArgument;
                    }

                    createdNode.PlannedConstructorArguments = new List<IUnresolvedArgument>(slots);

                    foreach (var argument in createdNode.PlannedConstructorArguments)
                    {
                        switch (argument.ArgumentType)
                        {
                            case UnresolvedArgumentType.Type:
                                if (argument.IsMultipleResult)
                                {
                                    var children = CreatePlans(
                                        argument.MultipleResultElementType,
                                        createdNode,
                                        argument.ParameterName,
                                        planName,
                                        planRoot,
                                        null);
                                    foreach (var child in children)
                                    {
                                        if (child.ParentPlan == createdNode)
                                        {
                                            if (!createdNode.ChildrenInternal.Contains((INode)child))
                                            {
                                                createdNode.ChildrenInternal.Add((INode)child);
                                            }
                                        }
                                    }
                                    ((DefaultUnresolvedArgument)argument).PlannedTargets = children;
                                }
                                else
                                {
                                    var child = CreatePlan(
                                        argument.UnresolvedType,
                                        createdNode,
                                        argument.ParameterName,
                                        planName,
                                        planRoot,
                                        null);
                                    if (child.ParentPlan == createdNode)
                                    {
                                        if (!createdNode.ChildrenInternal.Contains((INode)child))
                                        {
                                            createdNode.ChildrenInternal.Add((INode)child);
                                        }
                                    }
                                    ((DefaultUnresolvedArgument)argument).PlannedTarget = child;
                                }

                                break;
                        }
                    }

                    if (createdNode.Parent == null)
                    {
                        _hierarchy.RootNodes.Add(createdNode);
                    }
                    else
                    {
                        var childrenInternal = ((DefaultNode)scopeNode).ChildrenInternal;
                        if (!childrenInternal.Contains(createdNode))
                        {
                            childrenInternal.Add(createdNode);
                        }
                    }

                    plans[i] = createdNode;
                }
                finally
                {
                    planRoot.PlannedCreatedNodes.Add(createdNode);
                }
            }

            return plans;
        }

        private IMapping[] ResolveTypes(Type originalType, string name, INode current)
        {
            var mappings = new List<IMapping>();

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

                    mappings.Add(b);
                }
            }

            if (mappings.Count == 0)
            {
                if (originalType.IsGenericType && _bindings.ContainsKey(originalType.GetGenericTypeDefinition()))
                {
                    // Try the original generic type definition to see if we
                    // need to pass generic parameters through.
                    var bindings = _bindings[originalType.GetGenericTypeDefinition()];
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

                        mappings.Add(b);
                    }
                }
            }

            if (mappings.Count == 0)
            {
                // If the type is a concrete type, return it.
                if (!originalType.IsAbstract && !originalType.IsInterface)
                {
                    mappings.Add(new DefaultMapping
                    {
                        Target = originalType
                    });
                }
            }
            
            return mappings.ToArray();
        }

        #endregion
    }
}