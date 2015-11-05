using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protoinject
{
    internal class DefaultBindToInScopeWithDescendantFilterOrUnique<TInterface> :
        IBindToInScopeWithDescendantFilterOrUnique<TInterface>
    {
        private DefaultMapping _mapping;

        public DefaultBindToInScopeWithDescendantFilterOrUnique(DefaultMapping defaultMapping)
        {
            _mapping = defaultMapping;
        }

        public IBindInScopeWithDescendantFilterOrUnique To<T>() where T : TInterface
        {
            _mapping.Target = typeof(T);
            return this;
        }

        public IBindInScopeWithDescendantFilterOrUnique To(Type type)
        {
            _mapping.Target = type;
            return this;
        }

        public void InTransientScope()
        {
            // No change required.
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

        public IBindInScopeOrUnique WithDescendantFilter(INode descendantOf)
        {
            _mapping.OnlyUnderDescendantFilter = descendantOf;
            return this;
        }
    }
}
