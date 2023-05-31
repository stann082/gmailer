using core;
using core.cli;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace service;

public class EmailService : IEmailService
{

    #region Constructors

    public EmailService()
    {
        var credential = Authenticate().GetAwaiter().GetResult();
        _service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Gmailer"
        });
    }

    #endregion

    #region Variables

    private readonly GmailService _service;

    #endregion

    #region Public Methods

    public async Task ListEmails(ListOptions options)
    {
        List<MessageBatch> messages = new List<MessageBatch>();
        await LoadMessages(messages, "first", options);

        List<Task<Email[]>> tasks = new List<Task<Email[]>>();
        Parallel.ForEach(messages, batch => { tasks.Add(GetEmails(batch)); });
        var emailsTask = await Task.WhenAll(tasks.ToArray());

        Email[] emails = emailsTask.SelectMany(t => t).ToArray();
        emails.DetermineDomains();
        var groupedEmails = emails
            .Select(e => new
            {
                e.Address,
                e.Domain,
                Count = 1
            })
            .GroupBy(e => e.Domain)
            .OrderBy(g => g.Count());

        foreach (var group in groupedEmails)
        {
            int count = group.Sum(e => e.Count);
            string? address = group.First().Address;
            string output = $"{address} [{count}]";
            Console.WriteLine(output);
        }

        Console.WriteLine($"Total emails: {emails.Length}");
    }

    #endregion

    #region Helper Methods

    private static async Task<UserCredential> Authenticate()
    {
        string? clientId = Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID");
        string? clientSecret = Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET");
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new AggregateException("Client id or client secret aren't set");
        }

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            new[] { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify },
            "user",
            CancellationToken.None,
            new FileDataStore("etc"));
        return credential;
    }

    private async Task<Email[]> GetEmails(MessageBatch batch)
    {
        List<Email> emails = new List<Email>();

        foreach (var id in batch.MessageIds)
        {
            var request = _service.Users.Messages.Get("me", id);
            var response = await request.ExecuteAsync();
            if (response == null)
            {
                throw new AggregateException("Could not return emails.");
            }

            emails.Add(new Email(response.Payload.Headers, id));
        }

        return emails.ToArray();
    }

    private async Task LoadMessages(ICollection<MessageBatch> messages, string pageToken, ListOptions options)
    {
        if (string.IsNullOrEmpty(pageToken))
        {
            return;
        }

        var emailListRequest = _service.Users.Messages.List("me");
        emailListRequest.LabelIds = options.Label?.ToUpper();
        emailListRequest.IncludeSpamTrash = false;
        emailListRequest.MaxResults = options.MaxResults;
        emailListRequest.PageToken = pageToken != "first" ? pageToken : null;

        if (options.Unread)
        {
            emailListRequest.Q = "is:unread";
        }

        var response = await emailListRequest.ExecuteAsync();
        if (response?.Messages == null)
        {
            throw new AggregateException("Could not return messages.");
        }

        messages.Add(new MessageBatch(response.Messages));
        await LoadMessages(messages, response.NextPageToken, options);
    }

    #endregion

}
