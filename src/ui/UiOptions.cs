using core.nullobj;

namespace ui;

public class UiOptions : NullMessagesOptions
{
    
    public override bool IsDescending => true;
    public override string Label { get; set; } = "inbox";
    public override bool ShouldGetCache { get; set; } = true;
    public override bool ShouldCacheEmails { get; set; }
}
