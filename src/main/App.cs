using CommandLine;
using core.cli;
using service;

namespace main;

public class App
{
    
    #region Constructors

    public App(IEmailService emailService)
    {
        _emailService = emailService;
    }

    #endregion

    #region Variables

    private readonly IEmailService _emailService;

    #endregion

    #region Public Methods

    public int RunApp(IEnumerable<string> args)
    {
        return Parser.Default.ParseArguments<ListOptions,
                ComposeOptions>(args)
            .MapResult(
                (ListOptions opts) => ListEmails(opts),
                (ComposeOptions opts) => ComposeEmail(opts),
                errs => 1);
    }

    #endregion

    #region Helper Methods

    private int ComposeEmail(ComposeOptions opts)
    {
        throw new NotImplementedException();
    }

    private int ListEmails(ListOptions opts)
    {
        _emailService.ListEmails(opts);
        return 0;
    }

    #endregion

}
