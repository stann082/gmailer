using cli.options;
using CommandLine;
using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;
using service;

namespace cli;

public class App(IEmailService emailService)
{

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
            IEnumerable<Label> labels = emailService.ListLabels().GetAwaiter().GetResult();
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

    private int ListEmails(IMessagesOptions opts)
    {
        if (opts.Label == "all")
        {
            Console.WriteLine("Trying to fetch all messages may result in a rate limit exception. Use at your own risk.");
        }

        if (opts.Recent > 500)
        {
            Console.WriteLine($"The number of recent items to display {opts.Recent} cannot be greater than 500");
            return 1;
        }

        EmailGroupingCollection grouping = emailService.ListEmails(opts).GetAwaiter().GetResult();
        if (opts.ShouldCacheEmails)
        {
            Console.WriteLine($"Cached {grouping.GetEmailsTotal()} emails");
            return 0;
        }
        
        if (!opts.ShouldGroup)
        {
            foreach (var email in grouping.GetEmails().OrderBy(e => e.ToDateTime()))
            {
                Console.WriteLine($"{email.Subject} <{email.Address}> [{email.ToDateTime()}]");
            }

            return 0;
        }

        int count = 1;
        foreach (var group in grouping.GetGroupings())
        {
            string output = $"{count}: {group.Domain} ({group.Total})";
            Console.WriteLine(output);
            count++;
        }

        Console.WriteLine($"Total emails: {grouping.GetEmailsTotal()}");
        return 0;
    }

    #endregion

}
