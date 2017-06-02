﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
#if !PLATFORM_UNITY
using System.Runtime.ExceptionServices;
using System.Collections.ObjectModel;
#endif
#if !PLATFORM_UNITY
using System.Threading.Tasks;
#endif
#if PLATFORM_UNITY
using Protoinject.UnityClasses;
#endif

namespace Protoinject
{
    public partial class StandardKernel : IKernel
    {
        private Dictionary<Type, List<IMapping>> _bindings;

        private IHierarchy _hierarchy;

        private IScope _singletonScope;

        private Dictionary<Assembly, Type[]> _assemblyTypeCache;

        public StandardKernel()
        {
            _bindings = new Dictionary<Type, List<IMapping>>();
            _hierarchy = new DefaultHierarchy();
            _assemblyTypeCache = new Dictionary<Assembly, Type[]>();
        }

        public IHierarchy Hierarchy
        {
            get { return _hierarchy; }
        }

        public IReadOnlyDictionary<Type, IReadOnlyList<IMapping>> GetCopyOfBindings()
        {
            var copyDict = _bindings.ToDictionary(k => k.Key, v => (IReadOnlyList<IMapping>)v.Value);
            return new ReadOnlyDictionary<Type, IReadOnlyList<IMapping>>(copyDict);
        }

        public void Load<T>() where T : IProtoinjectModule
        {
            Activator.CreateInstance<T>().Load(this);
        }

        public void Load(IProtoinjectModule module)
        {
            module.Load(this);
        }

        public IBindToInScopeWithDescendantFilterOrUniqueOrNamed<TInterface> Bind<TInterface>()
        {
            List<IMapping> list;
            if (!_bindings.ContainsKey(typeof(TInterface)))
            {
                list = new List<IMapping>();
                _bindings[typeof(TInterface)] = list;
            }
            else
            {
                list = _bindings[typeof(TInterface)];
            }
            var mapping = new DefaultMapping();
            mapping.Target = typeof(TInterface);
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

        public void UnbindSpecific<T>(Func<IMapping, bool> unbindFilter)
        {
            UnbindSpecific(typeof(T), unbindFilter);
        }

        public void UnbindSpecific(Type type, Func<IMapping, bool> unbindFilter)
        {
            if (_bindings.ContainsKey(type))
            {
                foreach (var b in _bindings[type].ToArray())
                {
                    if (unbindFilter(b))
                    {
                        _bindings[type].Remove(b);
                    }
                }
            }
        }

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
                _hierarchy.AddRootNode(node);
            }
            else
            {
                _hierarchy.AddChildNode(parent, node);
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

        public void Validate(IPlan plan)
        {
#if !PLATFORM_UNITY
            AsyncHelpers.RunSync(() => ValidateAsync(plan));
        }

        public async Task ValidateAsync(IPlan plan)
        {
#endif
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

                if (toCreate.PlanRoot == toCreate)
                {
                    throw new ActivationException(
                        "The planned node has itself as the root plan.  This can occur when you " +
                        "have manually constructed the plan, in which case you must ensure that " +
                        "all child plans of the root plan have the root plan correctly set.",
                        plan);
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
#if !PLATFORM_UNITY
                                    for (int index = 0; index < argument.PlannedTargets.Length; index++)
#else
                                    for (int index = 0; index < argument.PlannedTargets.Count; index++)
#endif
                                    {
                                        var target = argument.PlannedTargets[index];

                                        ValidateArgument(argument, target);
                                    }
                                }
                                else
                                {
                                    ValidateArgument(argument, argument.PlannedTarget);
                                }
                                break;
                        }
                    }
                }
            }

            // TODO: Validate more configuration
        }

        public void Discard(IPlan plan)
        {
#if !PLATFORM_UNITY
            AsyncHelpers.RunSync(() => DiscardAsync(plan));
        }

        public async Task DiscardAsync(IPlan plan)
        {
#endif
            var planAsNode = (DefaultNode)plan;
            planAsNode.Discarded = true;
            foreach (var plan1 in planAsNode.PlannedCreatedNodes)
            {
                var toCreate = (DefaultNode)plan1;
                var parent = toCreate.ParentPlan;
                if (parent != null)
                {
                    _hierarchy.RemoveChildNode(parent, toCreate);
                }
                else
                {
                    _hierarchy.RemoveRootNode(toCreate);
                }
                toCreate.Parent = null;
                toCreate.Discarded = true;
            }
            var nodeParent = planAsNode.ParentPlan;
            if (nodeParent != null)
            {
                _hierarchy.RemoveChildNode(nodeParent, planAsNode);
            }
            else
            {
                _hierarchy.RemoveRootNode(planAsNode);
            }

            if (plan.DiscardOnResolve.Count > 0)
            {
                foreach (var child in plan.DiscardOnResolve.ToList())
                {
                    _hierarchy.RemoveNode((INode)child);
                }
            }
        }

        public INode ResolveToNode(IPlan plan)
        {
#if !PLATFORM_UNITY
            return AsyncHelpers.RunSync(() => ResolveToNodeAsync(plan));
        }

        public async Task<INode> ResolveToNodeAsync(IPlan plan)
        {
#endif
            if (!plan.Planned)
            {
                return (INode)plan;
            }

            foreach (var dependent in plan.DependentOnPlans)
            {
                if (dependent.Discarded)
                {
                    throw new ActivationException("This plan was dependent on plan '" + dependent.FullName + "' / '" +
                                                  dependent.PlanName +
                                                  "', but that plan has since been discarded.  Re-create this plan to resolve it.",
                        plan);
                }
                else if (dependent.Planned)
                {
                    throw new ActivationException("This plan is dependent on plan '" + dependent.FullName + "' / '" +
                                                  dependent.PlanName + "', but that plan is not resolved yet.", plan);
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
                    _hierarchy.ChangeObjectOnNode(
                        toCreate,
                        toCreate.PlannedMethod(new DefaultContext(this, ((DefaultNode)node).Parent, node)));
                }
                else if (toCreate.Planned && toCreate.Deferred)
                {
                    // Remove this deferred node from the tree, because the
                    // resolved target node will be used by ResolveArgument
                    // to provide any parameter values as needed.
                    _hierarchy.RemoveNode(toCreate);
                }
                else if (toCreate.Planned)
                {
                    var parameters = new List<object>();
                    foreach (var argument in toCreate.PlannedConstructorArguments)
                    {
#if !PLATFORM_UNITY
                        parameters.Add(await ResolveArgument(toCreate, argument));
#else
                        parameters.Add(ResolveArgument(toCreate, argument));
#endif
                    }
                    try
                    {
                        // Create the uninitialized object so that node lookups work before the constructors
                        // have finished being called.
                        toCreate.UntypedValue = FormatterServices.GetUninitializedObject(toCreate.Type);

                        _hierarchy.ChangeObjectOnNode(toCreate, toCreate.UntypedValue);

                        // If the object being created has [InjectFieldsForBaseObjectInProtectedConstructorAttribute]
                        // on any of it's base classes, inject them now.
                        var cls = toCreate.UntypedValue.GetType().BaseType;
                        while (cls != null)
                        {
                            var wantsFieldsInjected = cls.GetCustomAttributes(false).Any(
                                x => x is InjectFieldsForBaseObjectInProtectedConstructorAttribute);
                            if (wantsFieldsInjected)
                            {
                                var nodeField = cls.GetField("_node", BindingFlags.NonPublic | BindingFlags.Instance);
                                var hierarchyField = cls.GetField("_hierarchy", BindingFlags.NonPublic | BindingFlags.Instance);

                                if (nodeField.FieldType == typeof(INode))
                                {
                                    nodeField.SetValue(toCreate.UntypedValue, toCreate);
                                }

                                if (hierarchyField.FieldType == typeof(IHierarchy))
                                {
                                    hierarchyField.SetValue(toCreate.UntypedValue, _hierarchy);
                                }
                            }

                            cls = cls.BaseType;
                        }

                        toCreate.PlannedConstructor.Invoke(toCreate.UntypedValue, parameters.ToArray());

                        var asyncConstructable = toCreate.UntypedValue as IAsynchronouslyConstructable;
                        if (asyncConstructable != null)
                        {
                            await asyncConstructable.ConstructAsync();
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
#if !PLATFORM_UNITY
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
#else
                        throw ex.InnerException;
#endif
                    }
                    toCreate.Planned = false;
                }
            }

            if (plan.DiscardOnResolve.Count > 0)
            {
                foreach (var child in plan.DiscardOnResolve.ToList())
                {
                    _hierarchy.RemoveNode((INode)child);
                }
            }

            return (INode)plan;
        }

#if !PLATFORM_UNITY
        private async Task<object> ResolveArgument(DefaultNode toCreate, IUnresolvedArgument argument)
#else
        private object ResolveArgument(DefaultNode toCreate, IUnresolvedArgument argument)
#endif
        {
            switch (argument.ArgumentType)
            {
                case UnresolvedArgumentType.Type:
                    if (argument.IsMultipleResult)
                    {
#if !PLATFORM_UNITY
                        var value = (Array)Activator.CreateInstance(argument.MultipleResultElementType.MakeArrayType(), new object[] { argument.PlannedTargets.Length });
                        for (int index = 0; index < argument.PlannedTargets.Length; index++)
#else
                        var value = (Array)Activator.CreateInstance(argument.MultipleResultElementType.MakeArrayType(), new object[] { argument.PlannedTargets.Count });
                        for (int index = 0; index < argument.PlannedTargets.Count; index++)
#endif
                        {
                            var target = argument.PlannedTargets[index];

                            while (target.Deferred && target.DeferredResolvedTarget != null)
                            {
                                target = target.DeferredResolvedTarget;
                            }

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
                        var target = argument.PlannedTarget;

                        while (target.Deferred && target.DeferredResolvedTarget != null)
                        {
                            target = target.DeferredResolvedTarget;
                        }

                        if (target.Planned)
                        {
                            if (argument.InjectionParameters.OfType<OptionalAttribute>().Any())
                            {
                                return null;
                            }
                            else
                            {
                                throw new ActivationException(
                                    "Expected " + target.FullName + " to be resolved by now.", toCreate);
                            }
                        }
                        return ((DefaultNode)target).UntypedValue;
                    }
                case UnresolvedArgumentType.Factory:
                    return argument.FactoryDelegate;
                case UnresolvedArgumentType.FactoryArgument:
                    return argument.FactoryArgumentValue;
                case UnresolvedArgumentType.CurrentNode:
                    return argument.CurrentNode;
                case UnresolvedArgumentType.Node:
                    return argument.Node;
                case UnresolvedArgumentType.Hierarchy:
                    return argument.Hierarchy;
                case UnresolvedArgumentType.KnownValue:
                    return argument.KnownValue;
            }

            throw new ActivationException("Unexpected argument type", toCreate);
        }

#if !PLATFORM_UNITY
        private async Task<IPlan> CreatePlan(Type requestedType, INode current, string bindingName, string planName,
            INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments,
            Dictionary<Type, List<IMapping>> transientBindings)
#else
        private IPlan CreatePlan(Type requestedType, INode current, string bindingName, string planName,
            INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments,
            Dictionary<Type, List<IMapping>> transientBindings)
#endif
        {
#if !PLATFORM_UNITY
            var plans = await CreatePlans(requestedType, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
            if (plans.Length != 1)
#else
            var plans = CreatePlans(requestedType, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
            if (plans.Count != 1)
#endif
            {
                foreach (var plan in plans)
                {
                    Discard(plan);
                }

                var nodeToCreate = typeof(DefaultNode<>).MakeGenericType(requestedType);
                var createdNode = (DefaultNode)Activator.CreateInstance(nodeToCreate);
                createdNode.Parent = current;
                createdNode.Planned = true;

#if !PLATFORM_UNITY
                if (plans.Length == 0)
#else
                if (plans.Count == 0)
#endif
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

#if !PLATFORM_UNITY
        private async Task<IPlan[]> CreatePlans(
#else
        private List<IPlan> CreatePlans(
#endif
            Type requestedType, 
            INode current, 
            string bindingName, 
            string planName, 
            INode planRoot, 
            IInjectionAttribute[] injectionAttributes, 
            IConstructorArgument[] arguments,
            Dictionary<Type, List<IMapping>> transientBindings)
        {
            injectionAttributes = injectionAttributes ?? new IInjectionAttribute[0];
            arguments = arguments ?? new IConstructorArgument[0];

            // If the plan requires an existing node, we must defer resolution until the plan
            // has been fully formed.  For now, we create a node with Deferred set to
            // true, the requested type and desired scope node (if applicable).
            var requireExistingAttribute = injectionAttributes.OfType<RequireExistingAttribute>().FirstOrDefault();
            if (requireExistingAttribute != null)
            {
                if (planRoot == null)
                {
                    // We can't defer resolution on a plan root (it doesn't even make sense
                    // because there won't ever be anything to satisfy the request).
                    throw new InvalidOperationException(
                        "A plan root had a RequireExisting injection attribute, which " +
                        "can't ever be satisfied.");
                }

                var deferredSearchOptions = new List<KeyValuePair<Type, INode>>();

                // Add deferred search option if the current plan has a scope injection attribute.
                var scopeAttribute = injectionAttributes.OfType<ScopeAttribute>().FirstOrDefault();
                if (scopeAttribute != null)
                {
                    // We don't have a resolved mapping here, so we pass in null as the second argument.
                    var scopeNode = scopeAttribute.ScopeFromContext(current, null);
                    deferredSearchOptions.Add(new KeyValuePair<Type, INode>(
                        requestedType,
                        scopeNode));
                }

                // Add deferred search options based on explicit mappings in the kernel.
#if !PLATFORM_UNITY
                var requireResolvedMappings = await ResolveTypes(requestedType, bindingName, current, transientBindings);
                var requirePlans = (IPlan[])Activator.CreateInstance(typeof(IPlan<>).MakeGenericType(requestedType).MakeArrayType(), 1);
#else
                var requireResolvedMappings = ResolveTypes(requestedType, bindingName, current, transientBindings);
                var requirePlans = new List<IPlan>(1);
                requirePlans.Add(null);
#endif
                foreach (var mapping in requireResolvedMappings)
                {
                    // The mechanism of adding additional desired types based on the bindings
                    // can only work for bindings that provide both a known target type, and
                    // a lifetime scope.
                    if (mapping.Target != null && mapping.LifetimeScope != null)
                    {
                        deferredSearchOptions.Add(new KeyValuePair<Type, INode>(
                            mapping.Target,
                            mapping.LifetimeScope.GetContainingNode()));
                    }
                }

                // Create the deferred node.
                Type nodeToCreate = typeof(DefaultNode<>).MakeGenericType(requestedType);
                var createdNode = (DefaultNode)Activator.CreateInstance(nodeToCreate);
                createdNode.Name = string.Empty;
                createdNode.Parent = current;
                createdNode.Planned = true;
                createdNode.Deferred = true;
#if PLATFORM_UNITY
                createdNode.DeferredSearchOptions = deferredSearchOptions.ToDictionary(k => k.Key, v => v.Value);
#else
                createdNode.DeferredSearchOptions = deferredSearchOptions.AsReadOnly();
#endif
                createdNode.PlanName = planName;
                createdNode.PlanRoot = planRoot;
                createdNode.RequestedType = requestedType;

                // Add it to the list of deferred nodes on the plan root.
                planRoot.DeferredCreatedNodes.Add(createdNode);

                // Set the required plans and return it.
                requirePlans[0] = createdNode;
                return requirePlans;
            }

            // Otherwise, construct plans based on the kernel configuration.
#if !PLATFORM_UNITY
            var resolvedMappings = await ResolveTypes(requestedType, bindingName, current, transientBindings);
            var plans = (IPlan[])Activator.CreateInstance(typeof(IPlan<>).MakeGenericType(requestedType).MakeArrayType(), resolvedMappings.Length);
#else
            var resolvedMappings = ResolveTypes(requestedType, bindingName, current, transientBindings);
            var plans = new List<IPlan>(resolvedMappings.Length);
#endif
            for (var i = 0; i < resolvedMappings.Length; i++)
            {
#if PLATFORM_UNITY
                plans.Add(null);
#endif

                var resolvedMapping = resolvedMappings[i];
                var localPlanRoot = planRoot;

                // If the resolved target is a generic type definition, we need to fill in the
                // generic type arguments from the request.
                var targetNonGeneric = resolvedMapping.Target;
                if (targetNonGeneric != null && targetNonGeneric.IsGenericTypeDefinition)
                {
#if !PLATFORM_UNITY
                    targetNonGeneric = targetNonGeneric.MakeGenericType(requestedType.GenericTypeArguments);
#else
                    targetNonGeneric = targetNonGeneric.MakeGenericType(requestedType.GetGenericArguments());
#endif
                }

                // Use the current node as the scope, unless the binding overrides the scope.
                var scopeNode = current;
                var uniquePerScope = resolvedMapping.UniquePerScope;
                if (resolvedMapping.LifetimeScope != null)
                {
                    scopeNode = resolvedMapping.LifetimeScope.GetContainingNode();
                }

                // If the parameter or injection location has a scope attribute, that overrides
                // the bindings default scope.
                var scopeAttribute = injectionAttributes.OfType<ScopeAttribute>().FirstOrDefault();
                if (scopeAttribute != null)
                {
                    scopeNode = scopeAttribute.ScopeFromContext(current, resolvedMapping);
                    uniquePerScope = scopeAttribute.UniquePerScope;
                }

                // If the binding is set to be unique per scope, find an existing plan for
                // this binding in the current scope if one exists.
                if (scopeNode != null && uniquePerScope)
                {
                    var existing =
                        scopeNode.Children.FirstOrDefault(x => x.Type != null && x.Type.IsAssignableFrom(targetNonGeneric));
                    if (existing != null)
                    {
                        if (existing.Planned && existing.PlanRoot != localPlanRoot)
                        {
                            // Flag that the plan root is now dependent on the other
                            // plan being resolved.
                            if (localPlanRoot != null)
                            {
                                localPlanRoot.DependentOnPlans.Add(existing.PlanRoot);
                            }
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
                createdNode.PlanRoot = localPlanRoot;
                createdNode.RequestedType = requestedType;
                
                if (createdNode.Type.ContainsGenericParameters)
                {
                    throw new InvalidOperationException("The type still contained generic type parameters even after initial binding resolution.");
                }

                // If there is no plan root, then we are the plan root.
                if (localPlanRoot == null)
                {
                    localPlanRoot = createdNode;
                }

                try
                {
                    if (resolvedMapping.DiscardNodeOnResolve)
                    {
                        // We discard this node from the hierarchy once the plan is resolved or discarded.
                        localPlanRoot.DiscardOnResolve.Add(createdNode);
                    }

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

                        var targetName = resolvedMapping.TargetFactoryNotSupported
                            ? attribute.NotSupportedFullTypeName
                            : attribute.FullTypeName;

#if !PLATFORM_UNITY
                        var resolvedFactoryClass = (await GetTypesForAssembly(createdNode.Type.Assembly))
#else
                        var resolvedFactoryClass = (GetTypesForAssembly(createdNode.Type.Assembly))
#endif
                                .FirstOrDefault(x => x.FullName == targetName);
                        if (resolvedFactoryClass == null)
                        {
                            // This node won't be valid because it's planned, has no value and
                            // has no constructor.
                            createdNode.InvalidHint = "The generated factory class '" + targetName +
                                                      "' could not be found in the assembly.";
                            plans[i] = createdNode;
                            continue;
                        }

                        // If the factory class is generic, pass in type parameters as needed.
                        if (resolvedFactoryClass != null && resolvedFactoryClass.IsGenericTypeDefinition)
                        {
#if !PLATFORM_UNITY
                            resolvedFactoryClass = resolvedFactoryClass.MakeGenericType(requestedType.GenericTypeArguments);
#else
                            resolvedFactoryClass = resolvedFactoryClass.MakeGenericType(requestedType.GetGenericArguments());
#endif
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

                        if (parameter.ParameterType == typeof (ICurrentNode))
                        {
                            plannedArgument.ArgumentType = UnresolvedArgumentType.CurrentNode;
                            plannedArgument.CurrentNode = new DefaultCurrentNode(createdNode);
                        }
                        else if (parameter.ParameterType == typeof(INode))
                        {
                            plannedArgument.ArgumentType = UnresolvedArgumentType.Node;
                            plannedArgument.Node = createdNode;
                        }
                        else if (parameter.ParameterType == typeof(IHierarchy))
                        {
                            plannedArgument.ArgumentType = UnresolvedArgumentType.Hierarchy;
                            plannedArgument.Hierarchy = _hierarchy;
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
                            plannedArgument.InjectionParameters =
                                parameters[ii].GetCustomAttributes(true).OfType<IInjectionAttribute>().ToArray();
                            var namedAttribute = plannedArgument.InjectionParameters.OfType<NamedAttribute>().FirstOrDefault();
                            plannedArgument.Name = namedAttribute == null ? null : namedAttribute.Name;
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
#if !PLATFORM_UNITY
                                    var children = await CreatePlans(
#else
                                    var children = CreatePlans(
#endif
                                        argument.MultipleResultElementType,
                                        createdNode,
                                        argument.Name,
                                        planName,
                                        localPlanRoot,
                                        argument.InjectionParameters,
                                        null,
                                        transientBindings);
                                    foreach (var child in children)
                                    {
                                        if (child.ParentPlan == createdNode)
                                        {
                                            _hierarchy.AddChildNode(createdNode, (INode)child);
                                        }
                                    }
                                    ((DefaultUnresolvedArgument)argument).PlannedTargets = children;
                                }
                                else
                                {
#if !PLATFORM_UNITY
                                    var child = await CreatePlan(
#else
                                    var child = CreatePlan(
#endif
                                        argument.UnresolvedType,
                                        createdNode,
                                        argument.Name,
                                        planName,
                                        localPlanRoot,
                                        argument.InjectionParameters,
                                        null,
                                        transientBindings);
                                    if (child.ParentPlan == createdNode)
                                    {
                                        _hierarchy.AddChildNode(createdNode, (INode)child);
                                    }
                                    ((DefaultUnresolvedArgument)argument).PlannedTarget = child;
                                }

                                break;
                        }
                    }

                    if (createdNode.Parent == null)
                    {
                        _hierarchy.AddRootNode(createdNode);
                    }
                    else
                    {
                        _hierarchy.AddChildNode(scopeNode, createdNode);
                    }

                    plans[i] = createdNode;
                }
                finally
                {
                    localPlanRoot.PlannedCreatedNodes.Add(createdNode);
                }
            }

            // If we are the plan root, go back through all of the nodes we deferred and try to
            // resolve them now that the plan has been fully created.
            if (planRoot != null && planRoot == current)
            {
                foreach (var deferred in planRoot.DeferredCreatedNodes)
                {
                    // Search the deferred options.
                    foreach (var searchOption in deferred.DeferredSearchOptions)
                    {
                        var type = searchOption.Key;
                        var scopeNode = searchOption.Value;

                        var existing =
                            scopeNode.Children.FirstOrDefault(x => x.Type != null && type.IsAssignableFrom(x.Type));
                        if (existing != null)
                        {
                            if (existing.Planned && existing.PlanRoot != planRoot)
                            {
                                // Flag that the plan root is now dependent on the other
                                // plan being resolved.
                                if (planRoot != null)
                                {
                                    planRoot.DependentOnPlans.Add(existing.PlanRoot);
                                }
                            }

                            // Set the existing node as the deferred target.
                            ((DefaultNode)deferred).DeferredResolvedTarget = existing;
                            break;
                        }
                    }

                    // If this deferred node doesn't have any deferred search options, give a
                    // more tailored error.
                    if (deferred.DeferredSearchOptions.Count == 0)
                    {
                        ((DefaultNode) deferred).InvalidHint =
                            "This node was deferred because it depends on an existing node " +
                            "being in the tree, however, no search options were provided on " +
                            "the deferred node.  This indicates that you have a parameter that " +
                            "specifies [RequireExisting], but has no explicit scope set on the " +
                            "parameter, and no explicit kernel bindings for '" + deferred.RequestedType + "' " +
                            "which also provide scopes.  The deferred node can not be resolved.";
                    }

                    // If we didn't find a resolved target for this deferred node, invalidate
                    // the deferred node.
                    if (deferred.DeferredResolvedTarget == null)
                    {
                        ((DefaultNode)deferred).InvalidHint =
                            "This node was deferred because it depends on an existing node " +
                            "being in the tree, however, no search options yielded a resolution " +
                            "for the node.  This usually indicates that an implementation was " +
                            "expecting you to declare a dependent service elsewhere in the " +
                            "hierarchy, but you haven't done so.  The request was looking for one of: \r\n" + 
                            deferred.DeferredSearchOptions
                                .Select(x => " * A '" + x.Key.FullName + "' within '" + x.Value.FullName + "'")
                                .Aggregate((a, b) => a + "\r\n" + b);
                    }
                }
            }

            return plans;
        }

#if !PLATFORM_UNITY
        private async Task<Type[]> GetTypesForAssembly(Assembly assembly)
#else
        private Type[] GetTypesForAssembly(Assembly assembly)
#endif
        {
            if (_assemblyTypeCache.ContainsKey(assembly))
            {
                return _assemblyTypeCache[assembly];
            }

            _assemblyTypeCache[assembly] = assembly.GetTypes();
            return _assemblyTypeCache[assembly];
        }

#if !PLATFORM_UNITY
        private async Task<IMapping[]> ResolveTypes(Type originalType, string name, INode current, Dictionary<Type, List<IMapping>> transientBindings)
#else
        private IMapping[] ResolveTypes(Type originalType, string name, INode current, Dictionary<Type, List<IMapping>> transientBindings)
#endif
        {
            var mappings = new List<IMapping>();

            // Try to resolve the type using bindings first.
            if (transientBindings != null && transientBindings.ContainsKey(originalType))
            {
                var bindings = transientBindings[originalType];
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
            else if (_bindings.ContainsKey(originalType))
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
                if (transientBindings != null && originalType.IsGenericType && transientBindings.ContainsKey(originalType.GetGenericTypeDefinition()))
                {
                    // Try the original generic type definition to see if we
                    // need to pass generic parameters through.
                    var bindings = transientBindings[originalType.GetGenericTypeDefinition()];
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
                else if (originalType.IsGenericType && _bindings.ContainsKey(originalType.GetGenericTypeDefinition()))
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
    }
}
 