using System;

#if PLATFORM_IOS
using UIKit;
#else
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endif

namespace Protoinject.Example
{
    public static class Program
    {
        public static void Main(string[] args)
        {
#if PLATFORM_IOS
            UIApplication.Main(args, null, "AppDelegate");
#else
            var kernel = new StandardKernel();

            var controllers = kernel.CreateScopeFromNode(kernel.CreateEmptyNode("Controllers"));
            kernel.Bind<IInput>().To<DefaultInput>().InScope(controllers).EnforceOnePerScope();
            kernel.Bind<IMovement>().To<DefaultMovement>().InScope(controllers).EnforceOnePerScope();

            var gameSessionNode = kernel.CreateEmptyNode("GameSession");
            var gameSession = kernel.CreateScopeFromNode(gameSessionNode);
            var networking = kernel.CreateScopeFromNode(kernel.CreateEmptyNode("Networking", gameSessionNode));
            var profiling = kernel.CreateScopeFromNode(kernel.CreateEmptyNode("Profiling", gameSessionNode));

            kernel.Bind<IProfiler>().To<DefaultProfiler>().InScope(profiling).EnforceOnePerScope();
            kernel.Bind<IProfilerUtil>().To<DefaultProfilerUtil>().InScope(profiling).EnforceOnePerScope();

            kernel.Bind<INetworkingSession>().To<NetworkingSession>().InScope(networking).EnforceOnePerScope();
            kernel.Bind<INetworkingPlayer>().To<NetworkingPlayer>().InScope(networking);

            kernel.Bind<IWorld>().To<DefaultWorld>();
            kernel.Bind<IPlayer>().To<Player>();

            var worldPlan = kernel.Plan<IWorld>(null, null, "world1", new IInjectionAttribute[0]);
            var worldPlan2 = kernel.Plan<IWorld>(null, null, "world2", new IInjectionAttribute[0]);
            //kernel.Validate(worldPlan);
            //var world = kernel.Resolve(worldPlan);

            Console.WriteLine("==== AFTER PLANNING TWO WORLDS ====");
            foreach (var root in kernel.Hierarchy.RootNodes)
            {
                Console.Write(root.GetDebugRepresentation());
            }

            /*
            kernel.Discard(worldPlan);
            
            Console.WriteLine("==== AFTER DISCARDING WORLD 1 ====");
            foreach (var root in kernel.Hierarchy.RootNodes)
            {
                Console.Write(root.GetDebugRepresentation());
            }
            */

            kernel.ResolveToNode(worldPlan);

            Console.WriteLine("==== AFTER RESOLVING WORLD 1 ====");
            foreach (var root in kernel.Hierarchy.RootNodes)
            {
                Console.Write(root.GetDebugRepresentation());
            }

            kernel.ResolveToNode(worldPlan2);

            Console.WriteLine("==== AFTER RESOLVING WORLD 2 ====");
            foreach (var root in kernel.Hierarchy.RootNodes)
            {
                Console.Write(root.GetDebugRepresentation());
            }
#endif
        }

        private static Random random = new Random();

        public static string GetRandomName()
        {
            return "inst" + random.Next();
        }
    }
}
