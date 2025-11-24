using CommandLine;
using core;
using core.interfaces;

namespace cli.options;

[Verb("messages", HelpText = "Managing messages.")]
public class MessagesOptions : AbstractOptions, IMessagesOptions
{

    #region Properties
    
    [Option("no-body", HelpText = "Do not include email body during caching.")]
    public bool DoNotIncludeBody { get; set; }

    public bool IsDescending => false;

    [Option('c', "cache", HelpText = "Cache emails.")]
    public bool ShouldCacheEmails { get; set; }

    [Option("label", Default = "inbox", HelpText = "Filter by label.")]
    public string Label { get => _label.ToUpper(); set => _label = value; }

    [Option('p', "page", SetName = "paging", HelpText = "How many results per page to show.")]
    public int ResultsPePage { get; set; }
    
    [Option('r', "recent", SetName = "recent", HelpText = "Show recent 'n' items.")]
    public int Recent { get; set; }

    [Option("get-cached", HelpText = "Get cached items.")]
    public bool ShouldGetCache { get; set; }
    
    [Option('g', "group", HelpText = "Group emails.")]
    public bool ShouldGroup { get; set; }

    [Option('u', "unread", HelpText = "Show unread emails only.")]
    public bool Unread { get; set; }

    #endregion

    #region Variables

    private string _label = string.Empty;

    #endregion
    
    #region Overridden Methods

    public override string GetCacheKey()
    {
        return $"{CacheKeyPrefix}{Label}";
    }

    #endregion

}
