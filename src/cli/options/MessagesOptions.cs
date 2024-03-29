﻿using CommandLine;
using core.interfaces;

namespace cli.options;

[Verb("messages", HelpText = "Managing messages.")]
public class MessagesOptions : IMessagesOptions
{

    [Option('c', "cache", Default = false, HelpText = "Cache emails.")]
    public bool ShouldCacheEmails { get; set; }
    
    [Option('d', "delete", Default = false, HelpText = "Deletes selected messages.")]
    public bool ShouldDelete { get; set; }
    
    [Option("label", Default = "inbox", HelpText = "Filter by label.")]
    public string? Label { get; set; }

    [Option('p', "page", SetName = "paging", HelpText = "How many results per page to show.")]
    public int ResultsPePage { get; set; }
    
    [Option('r', "recent", SetName = "recent", HelpText = "Show recent 'n' items.")]
    public int Recent { get; set; }

    [Option("get-cached", Default = false, HelpText = "Get cached items.")]
    public bool ShouldGetCache { get; set; }
    
    [Option('g', "group", Default = false, HelpText = "Group emails.")]
    public bool ShouldGroup { get; set; }

    [Option('u', "unread", Default = false, HelpText = "Show unread emails only.")]
    public bool Unread { get; set; }

}
