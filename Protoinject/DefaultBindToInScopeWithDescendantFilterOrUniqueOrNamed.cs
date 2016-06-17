using System;

namespace Protoinject
{
    internal class DefaultBindToInScopeWithDescendantFilterOrUnique<TInterface> :
        DefaultBindToInScopeWithDescendantFilterOrUniqueOrNamed,
        IBindToInScopeWithDescendantFilterOrUniqueOrNamed<TInterface>
    {
        IBindInScopeWithDescendantFilterOrUniqueOrNamed IBindTo<TInterface>.To<T>()
        {
            return To<T>();
        }

        public DefaultBindToInScopeWithDescendantFilterOrUnique(StandardKernel kernel, DefaultMapping defaultMapping) : base(kernel, defaultMapping)
        {
        }
    }

    internal class DefaultBindToInScopeWithDescendantFilterOrUniqueOrNamed :
        IBindToInScopeWithDescendantFilterOrUniqueOrNamed
    {
        private readonly StandardKernel _kernel;
        private DefaultMapping _mapping;

        public DefaultBindToInScopeWithDescendantFilterOrUniqueOrNamed(StandardKernel kernel, DefaultMapping defaultMapping)
        {
            _kernel = kernel;
            _mapping = defaultMapping;

            // This makes the most sense by default; when using ToFactory
            // it gets set to false implicitly.
            _mapping.UniquePerScope = true;
        }

        protected IBindInScopeWithDescendantFilterOrUniqueOrNamed To<T>()
        {
            _mapping.Target = typeof(T);
            return this;
        }

        IBindInScopeWithDescendantFilterOrUniqueOrNamed IBindToImplicit.To<T>()
        {
            return To<T>();
        }

        public IBindInScopeWithDescendantFilterOrUniqueOrNamed To(Type type)
        {
            _mapping.Target = type;
            return this;
        }

        public IBindInScopeWithDescendantFilterOrUniqueOrNamed ToMethod(Func<IContext, object> resolve)
        {
            _mapping.TargetMethod = resolve;
            return this;
        }

        public IBindInScopeWithDescendantFilterOrUniqueOrNamed ToFactory()
        {
            _mapping.TargetFactory = true;
            _mapping.TargetFactoryNotSupported = false;
            _mapping.UniquePerScope = false;
            return this;
        }

        public IBindInScopeWithDescendantFilterOrUniqueOrNamed ToFactoryNotSupported()
        {
            _mapping.TargetFactory = true;
            _mapping.TargetFactoryNotSupported = true;
            _mapping.UniquePerScope = false;
            return this;
        }

        public void InTransientScope()
        {
            // No change required.
        }

        public IBindUnique InSingletonScope()
        {
            _mapping.LifetimeScope = _kernel.GetSingletonScope();
            return this;
        }

        public IBindUnique InScope(IScope scope)
        {
            _mapping.LifetimeScope = scope;
            return this;
        }

        public void EnforceOnePerScope()
        {
            _mapping.UniquePerScope = true;
        }

        public void AllowManyPerScope()
        {
            _mapping.UniquePerScope = false;
        }

        public IBindInScopeOrUnique WithDescendantFilter(INode descendantOf)
        {
            _mapping.OnlyUnderDescendantFilter = descendantOf;
            return this;
        }

        public IBindInScopeWithDescendantFilterOrUnique Named(string name)
        {
            _mapping.Named = name;
            return this;
        }
    }
}
