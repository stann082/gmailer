namespace core.interfaces;

public interface IMessagesOptions
{
    
    bool IsDescending { get; }
    string? Label { get; }
    int ResultsPePage { get; }
    int Recent { get; }
    bool ShouldDelete { get; }
    bool ShouldCacheEmails { get; }
    bool ShouldGetCache { get; }
    bool ShouldGroup { get; }
    bool Unread { get; }
    
}
