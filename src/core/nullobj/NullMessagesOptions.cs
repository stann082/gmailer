using core.interfaces;

namespace core.nullobj;

public class NullMessagesOptions : IMessagesOptions
{

    public virtual bool ShouldCacheEmails { get; }
    public virtual string? Label { get; set; }
    public virtual int MaxResults { get; set;  }
    public virtual int Recent { get; }
    public virtual bool ShouldDelete { get; }
    public virtual bool ShouldGetCache { get; set; }
    public virtual bool ShouldGroup { get; set; }
    public virtual bool Unread { get; }
    
}
