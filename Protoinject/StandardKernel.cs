using System;
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

        private DefaultHierarchy _hierarchy;

        public StandardKernel()
        {
            _bindings = new Dictionary<Type, List<IMapping>>();
            _hierarchy = new DefaultHierarchy();
        }

        public IBindToInScopeWithDescendantFilterOrUnique<TInterface> Bind<TInterface>()
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
            return new DefaultBindToInScopeWithDescendantFilterOrUnique<TInterface>(mapping);
        }

        public IHierarchy Hierarchy => _hierarchy;

        public IPlan<T> Plan<T>(INode current = null)
        {
            return (IPlan<T>) Plan(typeof (T), current);
        }

        public IPlan Plan(Type t, INode current = null, object[] additionalConstructorObjects = null)
        {
            return CreatePlan(t, current, additionalConstructorObjects);
        }

        public void Validate<T>(IPlan<T> plan)
        {
            Validate((IPlan)plan);
        }

        public void Validate(IPlan plan)
        {
        }

        public T Resolve<T>(IPlan<T> plan)
        {
            return ResolveToNode(plan).Value;
        }

        public object Resolve(IPlan plan)
        {
            return ResolveToNode(plan).UntypedValue;
        }

        public INode<T> ResolveToNode<T>(IPlan<T> plan)
        {
            return (INode<T>) ResolveToNode((IPlan) plan);
        }

        public INode ResolveToNode(IPlan plan)
        {
            return null;
        }

        private IPlan CreatePlan(Type requestedType, INode current = null, object[] additionalConstructorObjects = null)
        {
            var resolvedMapping = ResolveType(requestedType, current);

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
                    return existing;
                }
            }

            var nodeToCreate = typeof (DefaultNode<>).MakeGenericType(resolvedMapping.Target);
            var createdNode = (DefaultNode) Activator.CreateInstance(nodeToCreate);
            createdNode.Name = string.Empty;
            createdNode.Parent = scopeNode;
            createdNode.Planned = true;
            createdNode.Type = resolvedMapping.Target;

            // If the type is System.Func or similar, create it as a factory.
            if (resolvedMapping.Target.FullName.StartsWith("System.Func`"))
            {
                createdNode.PlannedConstructor = null;
                createdNode.UntypedValue = CreateFuncFactory(createdNode.Type, current);
                return createdNode;
            }

            createdNode.PlannedConstructor =
                createdNode.Type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
            if (createdNode.PlannedConstructor == null)
            {
                // This node won't be valid because it's planned, has no value and
                // has no constructor.
                return createdNode;
            }

            createdNode.PlannedConstructorArguments = new List<IUnresolvedArgument>();

            var parameters = createdNode.PlannedConstructor.GetParameters();
            var paramCount = parameters.Length - (additionalConstructorObjects?.Length ?? 0);
            for (var i = 0; i < paramCount; i++)
            {
                var parameter = parameters[i];

                var plannedArgument = new DefaultUnresolvedArgument();
                plannedArgument.ParameterName = parameter.Name;

                if (parameter.ParameterType == typeof (ICurrentNode))
                {
                    plannedArgument.ArgumentType = UnresolvedArgumentType.CurrentNode;
                    plannedArgument.CurrentNode = new DefaultCurrentNode(createdNode);
                }
                else if (parameter.ParameterType.FullName.StartsWith("System.Func`"))
                {
                    plannedArgument.ArgumentType = UnresolvedArgumentType.Factory;
                    plannedArgument.UnresolvedType = parameter.ParameterType;
                    plannedArgument.FactoryDelegate = CreateFuncFactory(plannedArgument.UnresolvedType, current);
                    plannedArgument.FactoryType = plannedArgument.FactoryDelegate.Method.ReturnType;
                }
                else
                {
                    plannedArgument.ArgumentType = UnresolvedArgumentType.Type;
                    plannedArgument.UnresolvedType = parameters[i].ParameterType;
                }

                createdNode.PlannedConstructorArguments.Add(plannedArgument);
            }

            if (additionalConstructorObjects != null)
            {
                foreach (var additional in additionalConstructorObjects)
                {
                    var plannedArgument = new DefaultUnresolvedArgument();
                    plannedArgument.ArgumentType = UnresolvedArgumentType.FactoryArgument;
                    plannedArgument.FactoryArgumentValue = additional;
                    createdNode.PlannedConstructorArguments.Add(plannedArgument);
                }
            }

            foreach (var argument in createdNode.PlannedConstructorArguments)
            {
                switch (argument.ArgumentType)
                {
                    case UnresolvedArgumentType.Type:
                        var child = Plan(argument.UnresolvedType, createdNode);
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
                ((DefaultHierarchy) _hierarchy).AddRootNode(createdNode);
            }
            else if (scopeNode != current)
            {
                ((DefaultNode)scopeNode).ChildrenInternal.Add(createdNode);
            }

            return createdNode;
        }

        private IMapping ResolveType(Type originalType, INode current = null)
        {
            // Try to resolve the type using bindings first.
            if (_bindings.ContainsKey(originalType))
            {
                var bindings = _bindings[originalType];
                var sortedBindings = bindings.OrderBy(x => x.OnlyUnderDescendantFilter != null ? 0 : 1);
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
            throw new ActivationException("No matching bindings for type " + originalType.FullName, current);
        }

        private object FactoryResolve(Type typeToCreate, INode current, object[] parameters)
        {
            var plan = Plan(typeToCreate, current, parameters);
            Validate(plan);
            return Resolve(plan);
        }

        private Delegate CreateFuncFactory(Type factoryType, INode current = null)
        {
            var genericArguments = factoryType.GetGenericArguments();
            var funcArgumentCount = genericArguments.Length - 1;

            var targetType = genericArguments[funcArgumentCount];
            var get = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "FactoryResolve" && !x.IsGenericMethod);
            var args = new List<Expression>();
            args.Add(Expression.Constant(targetType, typeof(Type)));
            args.Add(Expression.Constant(current, typeof(INode)));
            var @params = new List<ParameterExpression>();
            for (var n = 0; n < funcArgumentCount; n++)
            {
                @params.Add(Expression.Parameter(genericArguments[n]));
            }
            args.Add(Expression.NewArrayInit(typeof (object), @params));
            var call = Expression.Call(Expression.Constant(this), get, args);
            var cast = Expression.Convert(call, targetType);
            var lambda = Expression.Lambda(cast, @params);
            return lambda.Compile();
        }

        /*


        List<IMapping> matchingBindings;
            if (!_bindings.ContainsKey(t))
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    matchingBindings = new List<IMapping>
                    {
                        new DefaultMapping
                        {
                            Target = t
                        }
                    };
                }
                else
                {
                }
            }
            else
            {
                matchingBindings = _bindings[t];
            }

            var sortedBindings = matchingBindings.OrderBy(x => x.OnlyUnderDescendantFilter != null ? 0 : 1);
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

                var target = b.Target;

                var scopeNode = current;
                if (b.LifetimeScope != null)
                {
                    scopeNode = b.LifetimeScope.GetContainingNode();
                }

                if (scopeNode != null && b.UniquePerScope)
                {
                    var existing = scopeNode.Children.FirstOrDefault(x => x.Type.IsAssignableFrom(target));
                    if (existing != null)
                    {
                        return existing.Value;
                    }
                }

                var constructor = target.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
                if (constructor == null)
                {
                    // TODO: exception / log
                    continue;
                }

                var self = new DefaultNode(scopeNode, null);
                self.SetTypeAndValue(target, null);

                var arguments = new List<object>();
                var parameters = constructor.GetParameters();
                var paramCount = parameters.Length - (additionalConstructorObjects?.Length ?? 0);
                for (var i = 0; i < paramCount; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.ParameterType == typeof (ICurrentNode))
                    {
                        arguments.Add(new DefaultCurrentNode(self));
                    }
                    else if (parameter.ParameterType.IsGenericType)
                    {
                        if (parameter.ParameterType.GetGenericTypeDefinition() == typeof (Func<>))
                        {
                            var targetType = parameter.ParameterType.GetGenericArguments()[0];
                            var get = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .First(x => x.Name == "Get" && !x.IsGenericMethod);
                            var args = new List<Expression>();
                            args.Add(Expression.Constant(this));
                            args.Add(Expression.Constant(targetType));
                            args.Add(Expression.Constant(self));
                            args.Add(Expression.NewArrayInit(typeof (object)));
                            var call = Expression.Call(Expression.Constant(this), get, args);
                            var ret = Expression.Return(Expression.Label(), call);
                            var lambda = Expression.Lambda(call);
                            arguments.Add(lambda.Compile());
                            //arguments.Add((Func<object>) (() => Get(targetType, self)));
                        }
                        else if (parameter.ParameterType.GetGenericTypeDefinition() == typeof (Func<,>))
                        {
                        }
                        else
                        {
                            arguments.Add(Get(parameter.ParameterType, self));
                        }
                    }
                    else
                    {
                        arguments.Add(Get(parameter.ParameterType, self));
                    }
                }

                if (additionalConstructorObjects != null)
                {
                    arguments.AddRange(additionalConstructorObjects);
                }

                var resolved = constructor.Invoke(arguments.ToArray());
                self.SetTypeAndValue(target, resolved);

                if (self.Parent == null)
                {
                    _rootHierarchies.Add(self);
                }
                else
                {
                    ((DefaultNode)self.Parent).ChildrenInternal.Add(self);
                }

                return resolved;
            }

            throw new ActivationException("No bindings matched the request", current);
        }
        */


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
                ((DefaultNode) parent).ChildrenInternal.Add(node);
            }
            return node;
        }
    }
}