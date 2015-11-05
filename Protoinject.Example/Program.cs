using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protoinject.Example
{
    public static class Program
    {
        public static void Main(string[] args)
        {
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

            var worldPlan = kernel.Plan<IWorld>();
            kernel.Validate(worldPlan);
            //var world = kernel.Resolve(worldPlan);

            foreach (var root in kernel.Hierarchy.RootNodes)
            {
                Console.Write(root.GetDebugRepresentation());
            }
        }

        private static Random random = new Random();

        public static string GetRandomName()
        {
            return "inst" + random.Next();
        }

        public static string GetDebugRepresentation(this INode current, string indent = null)
        {
            indent = indent ?? string.Empty;
            if (current == null)
            {
                return string.Empty;
            }
            var me = (indent + "* " + current.Name).TrimEnd();
            if (current.Type != null)
            {
                me += " (" + current.Type.FullName + ")";
            }
            if (current.Planned)
            {
                me += " **PLANNED**";
            }
            me += Environment.NewLine;
            foreach (var c in current.Children)
            {
                me += GetDebugRepresentation(c, indent + "  ");
            }
            if (current.Planned)
            {
                foreach (var p in current.PlannedConstructorArguments)
                {
                    me += GetDebugRepresentation(p, indent + "  ");
                }
            }
            return me;
        }

        private static string GetDebugRepresentation(IUnresolvedArgument current, string indent)
        {
            indent = indent ?? string.Empty;
            if (current == null)
            {
                return string.Empty;
            }
            var me = (indent + "- " + current.ParameterName).TrimEnd();
            me += " (" + current.ArgumentType.ToString() + ")";
            if (current.UnresolvedType != null)
            {
                me += " (" + current.UnresolvedType.FullName + ")";
            }
            if (current.PlannedTarget != null)
            {
                me += " -> " + current.PlannedTarget.FullName;
            }
            me += Environment.NewLine;
            return me;
        }
    }
}
