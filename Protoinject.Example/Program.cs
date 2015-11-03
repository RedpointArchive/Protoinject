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
            kernel.Bind<IInput, DefaultInput>(scope: controllers, reuse: true);
            kernel.Bind<IMovement, DefaultMovement>(scope: controllers, reuse: true);
            kernel.Bind<IWorld, DefaultWorld>();
            kernel.Bind<IPlayer, Player>();

            var world = kernel.Get<IWorld>();

            foreach (var root in kernel.GetRootHierarchies())
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
            var me = indent + "* " + current.Name;
            if (current.Type != null)
            {
                me += " (" + current.Type.FullName + ")";
            }
            me += Environment.NewLine;
            foreach (var c in current.Children)
            {
                me += GetDebugRepresentation(c, indent + "  ");
            }
            return me;
        }
    }

    public interface IWorld
    {
    }

    public class DefaultWorld : IWorld
    {
        public DefaultWorld(Func<string, Player> playerFactory, ISetNodeName setNodeName)
        {
            playerFactory("Player1");
            playerFactory("Player2");
        }
    }

    public interface IInventory
    {
    }

    public class DefaultInventory
    {
    }

    public interface IInput
    {
    }

    public class DefaultInput : IInput
    {
    }

    public interface IMovement
    {
    }

    public class DefaultMovement : IMovement
    {
        public DefaultMovement(IInput input, ISetNodeName setNodeName)
        {
            setNodeName.SetName(Program.GetRandomName());
        }
    }

    public interface IEntity
    {
    }

    public class Entity : IEntity
    {
    }

    public interface IPlayer
    { }

    public class Player : Entity, IPlayer
    {
        public Player(IMovement movement, ISetNodeName setNodeName, string name)
        {
            setNodeName.SetName(name);
        }
    }
}
