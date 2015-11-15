using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protoinject.Test
{
    using Protoinject.Example;
    using Prototest.Library.Version1;

    public class GenericFactoryTests
    {
        private readonly IAssert _assert;

        public GenericFactoryTests(IAssert assert)
        {
            this._assert = assert;
        }

        private IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind(typeof(IGenericFactory<,>)).ToFactory();
            kernel.Bind(typeof(IGeneric<,>)).To(typeof(DefaultGeneric<,>));
            kernel.Bind<IPlayer>().To<Player>();
            kernel.Bind<INetworkingPlayer>().To<NetworkingPlayer>();
            kernel.Bind<IMovement>().To<DefaultMovement>();
            kernel.Bind<IInput>().To<DefaultInput>();
            return kernel;
        }

        public void FactoryInterfaceDoesNotResolveToNull()
        {
            var kernel = CreateKernel();

            var factory = kernel.Get<IGenericFactory<Player, DefaultWorld>>();

            _assert.NotNull(factory);
        }

        public void FactoryInterfaceIsGenerated()
        {
            var kernel = CreateKernel();

            var factory = kernel.Get<IGenericFactory<Player, DefaultWorld>>();

            _assert.NotNull(factory);
            _assert.True(factory.GetType().FullName.StartsWith("_GeneratedFactories"));
        }

        public void GeneratedFactoryCanSpawn()
        {
            var kernel = CreateKernel();

            var factory = kernel.Get<IGenericFactory<Player, DefaultWorld>>();

            _assert.NotNull(factory);

            var generic = factory.CreateGeneric(null, "hello");

            _assert.NotNull(generic);
            _assert.IsType<DefaultGeneric<Player, string>>(generic);
        }
    }
}
