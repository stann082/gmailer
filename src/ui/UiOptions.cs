using core.interfaces;

namespace ui;

public class UiOptions : IMessagesOptions
{

    #region Properties

    public bool IsDescending => true;
    public string Label { get; set; } = "inbox";
    public int ResultsPePage { get; set; }
    public int Recent { get; set;  }
    public bool ShouldGetCache { get; set; } = true;
    public bool ShouldGroup { get; set; }
    public bool DoNotIncludeBody { get; set; }
    public bool Unread { get; set; }
    public bool ShouldCacheEmails { get; set; }

    #endregion

}
