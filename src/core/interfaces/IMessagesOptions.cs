namespace core.interfaces;

public interface IMessagesOptions
{
    
    bool IsDescending { get; }
    string? Label { get; }
    string MessageToDelete { get; }
    int ResultsPePage { get; }
    int Recent { get; }
    bool ShouldCacheEmails { get; }
    bool ShouldGetCache { get; }
    bool ShouldGroup { get; }
    bool Unread { get; }
    
}
