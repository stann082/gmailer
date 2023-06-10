namespace core.interfaces;

public interface IMessagesOptions
{
    
    bool Cache { get; }
    string? Label { get; }
    int MaxResults { get; }
    int Recent { get; }
    bool ShouldDelete { get; }
    bool ShouldGetCache { get; }
    bool Unread { get; }
    
}
