using core.interfaces;

namespace worker;

public class WorkerCacheOptions : ICacheOptions
{
    public bool DoNotIncludeBody => false;
    public bool ShouldClearCache => false;
}
