using System;

namespace Protoinject
{
    public interface IDynamicResolutionFallback
    {
        object GetInstance(Type interfaceType);
    }
}
