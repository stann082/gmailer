using CommandLine;

namespace core.cli;

[Verb("list", HelpText = "Listing emails.")]
public class ListOptions
{
    
    [Option('l', "label", Default = "inbox", HelpText = "Filter by label.")]
    public string Label { get; set; }
    
    [Option('p', "page", Default = 50, HelpText = "How many results per page to show.")]
    public int MaxResults { get; set; }
    
}
