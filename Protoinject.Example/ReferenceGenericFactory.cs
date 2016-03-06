using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protoinject.Example
{
    using Protoinject.Test;

    /// <summary>
    /// For reference inside ILSpy.
    /// </summary>
    public class ReferenceGenericFactory<T1, T3> : IGenericFactory<T1, T3> where T1 : class, IPlayer where T3 : IWorld
    {
        private readonly INode _node;
        private readonly IKernel _kernel;

        public ReferenceGenericFactory(ICurrentNode node, IKernel kernel)
        {
            _node = node.GetNodeForFactoryImplementation();
            _kernel = kernel;
        }

        public IGeneric<T1, T2> CreateGeneric<T2>(T1 a, T2 b) where T2 : IComparable<string>
        {
            return (IGeneric<T1, T2>)_kernel.Get(
                typeof(IGeneric<T1, T2>),
                _node,
                null,
                null,
                new IInjectionAttribute[0]);
        }
    }
}
