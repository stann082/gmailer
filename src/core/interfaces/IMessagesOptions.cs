﻿namespace core.interfaces;

public interface IMessagesOptions
{
    
    string? Label { get; }
    int MaxResults { get; }
    int Recent { get; }
    bool ShouldDelete { get; }
    bool ShouldCacheEmails { get; }
    bool ShouldGetCache { get; }
    bool ShouldGroup { get; }
    bool Unread { get; }
    
}
