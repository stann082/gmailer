using core.interfaces;

namespace ui;

public class UIOtions : IMessagesOptions
{
    public bool IsDescending => true;
    public string? Label { get; set; }
    public int ResultsPePage { get; }
    public int Recent { get; }
    public bool ShouldDelete { get; }
    public bool ShouldCacheEmails { get; }
    public bool ShouldGetCache { get; set; }
    public bool ShouldGroup { get; }
    public bool Unread { get; }
    
}
