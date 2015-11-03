using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Protoinject
{
    public class StandardKernel
    {
        private Dictionary<Type, List<IMapping>> _bindings;

        public List<INode> _rootHierarchies;

        public StandardKernel()
        {
            _bindings = new Dictionary<Type, List<IMapping>>();
            _rootHierarchies = new List<INode>();
        }

        public void Bind<TInterface, TImplementation>(INode descendantFilter = null, IScope scope = null, bool reuse = false) where TImplementation : TInterface
        {
            var list = _bindings.ContainsKey(typeof (TInterface))
                ? _bindings[typeof (TInterface)]
                : new List<IMapping>();
            list.Add(new DefaultMapping(typeof(TImplementation), descendantFilter, scope, reuse));
            if (list.Count == 1)
            {
                _bindings[typeof (TInterface)] = list;
            }
        }

        public IReadOnlyCollection<INode> GetRootHierarchies()
        {
            return _rootHierarchies.AsReadOnly();
        }

        public T Get<T>(INode current = null)
        {
            return (T)Get(typeof (T), current);
        }

        public object Get(Type t, INode current = null, object[] additionalConstructorObjects = null)
        {
            List<IMapping> matchingBindings;
            if (!_bindings.ContainsKey(t))
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    matchingBindings = new List<IMapping>
                    {
                        new DefaultMapping(t, null, null, false)
                    };
                }
                else
                {
                    throw new ActivationException("No matching bindings for type " + t.FullName, current);
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

                if (scopeNode != null && b.Reuse)
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
                    if (parameter.ParameterType == typeof (ISetNodeName))
                    {
                        arguments.Add(new DefaultSetNodeName(self));
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
                            var targetType = parameter.ParameterType.GetGenericArguments()[1];
                            var get = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .First(x => x.Name == "Get" && !x.IsGenericMethod);
                            var param0 = Expression.Parameter(parameter.ParameterType.GetGenericArguments()[0]);
                            var args = new List<Expression>();
                            args.Add(Expression.Constant(targetType));
                            args.Add(Expression.Constant(self));
                            args.Add(Expression.NewArrayInit(typeof (object), param0));
                            var call = Expression.Call(Expression.Constant(this), get, args);
                            var cast = Expression.Convert(call, targetType);
                            var ret = Expression.Return(Expression.Label(), call);
                            var lambda = Expression.Lambda(cast, param0);
                            arguments.Add(lambda.Compile());

                            //arguments.Add((Func<object, object>)(x => Get(targetType, self, new[] { x })));
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

        public IScope CreateScopeFromNode(INode node)
        {
            return new FixedScope(node);
        }

        public INode CreateEmptyNode(string name, INode parent = null)
        {
            var node = new DefaultNode(parent, name);
            if (parent == null)
            {
                _rootHierarchies.Add(node);
            }
            else
            {
                ((DefaultNode)parent).ChildrenInternal.Add(node);
            }
            return node;
        }

        private class FixedScope : IScope
        {
            private readonly INode _node;

            public FixedScope(INode node)
            {
                _node = node;
            }

            public INode GetContainingNode()
            {
                return _node;
            }
        }

        private class DefaultSetNodeName : ISetNodeName
        {
            private readonly INode _target;

            public DefaultSetNodeName(INode target)
            {
                _target = target;
            }

            public void SetName(string name)
            {
                _target.Name = name;
            }
        }

        private class DefaultNode : INode
        {
            public DefaultNode(INode parent, string name)
            {
                Parent = parent;
                Name = (name ?? string.Empty).Replace("/", "");
                ChildrenInternal = new List<INode>();
            }

            public void SetTypeAndValue(Type type, object value)
            {
                Type = type;
                Value = value;
            }

            internal List<INode> ChildrenInternal { get; }

            public INode Parent { get; }
            public string Name { get; set; }

            public IReadOnlyCollection<INode> Children => ChildrenInternal.AsReadOnly();

            public string FullName
            {
                get
                {
                    return
                        GetParents()
                            .Concat(new[] {this})
                            .Select(x => string.IsNullOrWhiteSpace(x.Name) ? ("(" + x.Type.FullName + ")") : x.Name)
                            .Aggregate((a, b) => a + "/" + b);
                }
            }

            public object Value { get; private set; }
            public Type Type { get; private set; }

            public T GetValue<T>()
            {
                return (T) Value;
            }

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
        }

        private class DefaultMapping : IMapping
        {
            public DefaultMapping(Type target, INode descendantFilter, IScope lifetimeScope, bool reuse)
            {
                Target = target;
                OnlyUnderDescendantFilter = descendantFilter;
                LifetimeScope = lifetimeScope;
                Reuse = reuse;
            }

            public Type Target { get; }
            public INode OnlyUnderDescendantFilter { get; }
            public IScope LifetimeScope { get; }
            public bool Reuse { get; }
        }
    }
}