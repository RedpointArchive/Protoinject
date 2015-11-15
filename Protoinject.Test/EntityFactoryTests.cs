using Protoinject.Example;
using Prototest.Library.Version1;

namespace Protoinject.Test
{
    public class EntityFactoryTests
    {
        private readonly IAssert _assert;

        public EntityFactoryTests(IAssert assert)
        {
            _assert = assert;
        }

        private IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IEntityFactory>().ToFactory();
            kernel.Bind<IPlayer>().To<Player>();
            kernel.Bind<INetworkingPlayer>().To<NetworkingPlayer>();
            kernel.Bind<IMovement>().To<DefaultMovement>();
            kernel.Bind<IInput>().To<DefaultInput>();
            return kernel;
        }

        public void FactoryInterfaceDoesNotResolveToNull()
        {
            var kernel = CreateKernel();

            var factory = kernel.Get<IEntityFactory>();

            _assert.NotNull(factory);
        }

        public void FactoryInterfaceIsGenerated()
        {
            var kernel = CreateKernel();

            var factory = kernel.Get<IEntityFactory>();

            _assert.NotNull(factory);
            _assert.True(factory.GetType().FullName.StartsWith("_GeneratedFactories"));
        }

        public void GeneratedFactoryCanSpawn()
        {
            var kernel = CreateKernel();

            var factory = kernel.Get<IEntityFactory>();

            _assert.NotNull(factory);

            var player = factory.CreatePlayer("test");

            _assert.NotNull(player);
            _assert.Equal("test", player.Name);
        }
    }
}