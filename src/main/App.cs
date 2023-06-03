using CommandLine;
using core;
using core.cli;
using Google.Apis.Gmail.v1.Data;
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
        return Parser.Default.ParseArguments<MessagesOptions,
                ComposeOptions,
                LabelsOptions>(args)
            .MapResult(
                (MessagesOptions opts) => ListEmails(opts),
                (ComposeOptions opts) => ComposeEmail(opts),
                (LabelsOptions opts) => Labels(opts),
                errs => 1);
    }

    #endregion

    #region Helper Methods

    private int ComposeEmail(ComposeOptions opts)
    {
        throw new NotImplementedException();
    }

    private int Labels(LabelsOptions opts)
    {
        try
        {
            Label[] labels = _emailService.ListLabels();
            foreach (Label label in labels)
            {
                Console.WriteLine(label.Name);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }

        return 0;
    }

    private int ListEmails(MessagesOptions opts)
    {
        if (opts.Label == "all")
        {
            Console.WriteLine("Trying to fetch all messages results in a rate limit exception. Do not use it until you figure out how to handle it.");
            return 1;
        }
        
        if (opts.Recent > opts.MaxResults)
        {
            Console.WriteLine($"The number of recent items to display {opts.Recent} cannot be greater than the number of results per page {opts.MaxResults}");
            return 1;
        }
        
        Email[] emails = _emailService.ListEmails(opts);
        var groupedEmails = emails
            .GroupBy(e => e.Domain)
            .OrderBy(g => g.Count());
        
        int count = 1;
        foreach (var group in groupedEmails)
        {
            int total = group.Count();
            var item = group.First();
            string? name = !string.IsNullOrEmpty(item.Name) ? item.Name : item.Address;
            string output = $"{count}: {name} ({total})";
            Console.WriteLine(output);
            count++;
        }

        Console.WriteLine($"Total emails: {emails.Length}");
        if (opts.ShouldDelete)
        {
            Console.Write("\nPlease enter the group numbers of emails you wish to delete: ");
            string? input = Console.ReadLine();
        }
        
        return 0;
    }

    #endregion

}
