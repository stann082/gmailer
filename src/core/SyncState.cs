namespace core;

public class SyncState
{
    #region Properties

    public string Id { get; set; } = Constants.SyncTimestampId;
    public DateTime LastSyncUtc { get; set; }

    #endregion
}
