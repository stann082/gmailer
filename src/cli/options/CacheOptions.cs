using CommandLine;
using core.interfaces;

namespace cli.options;

[Verb("cache", HelpText = "Caching messages.")]
public class CacheOptions : ICacheOptions
{
    
    [Option("no-body", HelpText = "Do not include email body during caching.")]
    public bool DoNotIncludeBody { get; set; }

}
