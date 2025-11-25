namespace core;

public class SyncState
{
    #region Properties

    public string Id { get; set; } = Constants.SyncStateId;
    public HashSet<string> SyncedIds { get; set; } = new();
    public DateTime? LastSyncUtc { get; set; }

    #endregion
}
