using cli.options;
using CommandLine;
using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;
using Serilog;
using service;

namespace cli;

public class App(IEmailService emailService)
{

    #region Public Methods

    public async Task<int> RunApp(IEnumerable<string> args)
    {
        await emailService.InitializeAsync();
        return await Parser.Default.ParseArguments<MessagesOptions, ComposeOptions, LabelsOptions>(args).MapResult(
                async (MessagesOptions opts) => await ListEmails(opts),
                async (ComposeOptions opts) => await ComposeEmail(opts),
                async (LabelsOptions opts) => await Labels(opts),
                _ => Task.FromResult(1));
    }

    #endregion

    #region Helper Methods

    private async Task<int> ComposeEmail(ComposeOptions opts)
    {
        return await Task.FromResult(0);
    }

    private async Task<int> Labels(LabelsOptions opts)
    {
        try
        {
            IEnumerable<Label> labels = await emailService.ListLabelsAsync();
            foreach (Label label in labels)
            {
                Log.Information("{LabelName}", label.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error occurred while listing labels:");
            return 1;
        }

        return 0;
    }

    private async Task<int> ListEmails(IMessagesOptions opts)
    {
        if (opts.Label == "all")
        {
            Log.Warning("Trying to fetch all messages may result in a rate limit exception. Use at your own risk");
        }

        if (opts.Recent > 500)
        {
            Log.Error("The number of recent items to display {Recent} cannot be greater than 500", opts.Recent);
            return 1;
        }

        EmailGroupingCollection grouping = await emailService.ListEmailsAsync(opts);
        if (opts.ShouldCacheEmails)
        {
            Log.Information("Cached {EmailsTotal} emails", grouping.GetEmailsTotal());
            return 0;
        }
        
        if (!opts.ShouldGroup)
        {
            foreach (var email in grouping.GetEmails().OrderBy(e => e.ToDateTime()))
            {
                Log.Information("{EmailSubject} <{EmailAddress}> [{EmailDate}]", email.Subject, email.Address, email.ToDateTime());
            }

            return 0;
        }

        int count = 1;
        foreach (var group in grouping.Groupings)
        {
            string output = $"{count}: {group.Domain} ({group.Total})";
            Log.Information("{Output}", output);
            count++;
        }

        Log.Information("{TotalEmails}: ", grouping.GetEmailsTotal());
        return 0;
    }

    #endregion

}
