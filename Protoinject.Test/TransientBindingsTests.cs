using System;
using System.Collections.Generic;
using Protoinject.Example;
using Prototest.Library.Version1;

namespace Protoinject.Test
{
    public class TransientBindingsTests
    {
        private readonly IAssert _assert;

        public TransientBindingsTests(IAssert assert)
        {
            _assert = assert;
        }

        public void TransientBindingsWork()
        {
            var kernel = new StandardKernel();

            kernel.Get(
                typeof (IInput),
                null,
                null,
                null,
                null,
                null,
                new Dictionary<Type, List<IMapping>>
                {
                    {
                        typeof (IInput), new List<IMapping>
                        {
                            new DefaultMapping(
                                typeof(DefaultInput),
                                null,
                                false,
                                null,
                                null,
                                false,
                                false,
                                null)
                        }
                    }
                });
        }
    }
}
