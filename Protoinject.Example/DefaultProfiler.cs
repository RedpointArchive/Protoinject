namespace Protoinject.Example
{
    public class DefaultProfiler : IProfiler
    {
        private readonly IProfilerUtil _profilerUtil;
        public DefaultProfiler(IProfilerUtil profilerUtil)
        {
            _profilerUtil = profilerUtil;
        }
    }
}