using core.interfaces;

namespace core.nullobj;

public class NullMessagesOptions : IMessagesOptions
{

    public virtual bool ShouldCacheEmails { get; set; }
    public virtual string? Label { get; set; }
    public virtual int ResultsPePage { get; set;  }
    public virtual int Recent { get; set; }
    public virtual bool ShouldDelete { get; set; }
    public virtual bool ShouldGetCache { get; set; }
    public virtual bool ShouldGroup { get; set; }
    public virtual bool Unread { get; set; }
    
}
