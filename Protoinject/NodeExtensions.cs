using System;

namespace Protoinject
{
    public static class NodeExtensions
    {
        public static string GetDebugRepresentation(this IPlan current, string indent = null)
        {
            return ((INode) current).GetDebugRepresentation(indent);
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
                if (!string.IsNullOrWhiteSpace(current.PlanName))
                {
                    me += " **PLANNED (as '" + current.PlanName + "')**";
                }
                else
                {
                    me += " **PLANNED**";
                }
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
            me += " (" + current.ArgumentType + ")";
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