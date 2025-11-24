using MongoDB.Bson.Serialization.Attributes;

namespace core;

public class SyncState
{
    #region Properties

    [BsonId]
    public string? Id { get; } = string.Empty;
    public DateTime LastSyncUtc { get; set; }
    public string Label { get; set; } = string.Empty;

    #endregion
}
