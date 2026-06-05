using cli.options;
using CommandLine;
using core;
using Google.Apis.Gmail.v1.Data;
using Serilog;
using service;

namespace cli;

public class CliApp(IEmailService emailService)
{
    
    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        await emailService.InitializeAsync();
        return await Parser.Default.ParseArguments<CacheOptions, MessagesOptions, LabelsOptions>(args).MapResult(
            async (CacheOptions opts) => await Cache(opts),
            async (MessagesOptions opts) => await ListEmails(opts),
            async (LabelsOptions opts) => await Labels(opts),
            _ => Task.FromResult(1));
    }

    #endregion

    #region Helper Methods

    private async Task<int> Cache(CacheOptions opts)
    {
        await emailService.CacheEmailsAsync(opts);
        return await Task.FromResult(0);
    }

    private async Task<int> Labels(LabelsOptions opts)
    {
        try
        {
            IEnumerable<Label> labels = await emailService.ListLabelsAsync();
            foreach (Label label in labels)
            {
                Console.WriteLine($"Id: {label.Id}; Name: {label.Name}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error occurred while listing labels:");
            return 1;
        }

        return await Task.FromResult(0);
    }

    private async Task<int> ListEmails(MessagesOptions opts)
    {
        EmailGroupingCollection grouping = emailService.ListEmails(opts);
        int count = 1;
        foreach (var group in grouping.Groupings)
        {
            string output = $"{count}: {group.Domain} ({group.Total})";
            Console.WriteLine($"{output}");
            count++;
        }

        Console.WriteLine($"Total emails: {grouping.GetEmailsTotal()}");
        return await Task.FromResult(0);
    }

    #endregion
}
