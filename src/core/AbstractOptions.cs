namespace core;

public abstract class AbstractOptions()
{

    #region Constants

    protected const string CacheKeyPrefix = "gmail:";

    #endregion

    #region Public Methods

    public abstract string GetCacheKey();

    #endregion

}
