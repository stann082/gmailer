using core.interfaces;

namespace core.nullobj;

public class NullMessagesOptions : IMessagesOptions
{

    public virtual bool IsDescending { get; set;  } = false;
    public virtual string Label { get; set; } = string.Empty;
    public virtual int ResultsPePage { get; set; } = 0;
    public virtual int Recent { get; set; } = 0;
    public virtual bool ShouldCacheEmails { get; set; } = false;
    public virtual bool ShouldGetCache { get; set; } = true;
    public virtual bool ShouldGroup { get; set; } = true;
    public virtual bool Unread { get; set; } = true;
    
}
