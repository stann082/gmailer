using CommandLine;
using core.interfaces;

namespace cli.options;

[Verb("messages", HelpText = "Managing messages.")]
public class MessagesOptions : IMessagesOptions
{

    #region Properties
    
    public bool IsDescending => false;

    [Option("label", Default = "inbox", HelpText = "Filter by label.")]
    public string Label { get => _label.ToUpper(); set => _label = value; }

    #endregion

    #region Variables

    private string _label = string.Empty;

    #endregion
    
}
