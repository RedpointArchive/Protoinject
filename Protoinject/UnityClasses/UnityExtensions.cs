#if PLATFORM_UNITY

using System;

namespace Protoinject.UnityClasses
{
    public static class UnityExtensions
    {
        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            foreach (var attr in type.GetCustomAttributes(false))
            {
                if (attr is T)
                {
                    return (T)attr;
                }
            }
            return null;
        }
    }
}

#endif