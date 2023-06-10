using core;
using core.interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace service;

public class EmailService : IEmailService
{

    #region Constructors

    public EmailService(IConnectionMultiplexer redis)
    {
        _cache = redis.GetDatabase();

        var credential = Authenticate().GetAwaiter().GetResult();
        _service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Gmailer"
        });
    }

    #endregion

    #region Variables

    private readonly IDatabase _cache;
    private readonly GmailService _service;

    #endregion

    #region Public Methods

    public EmailGroupingCollection ListEmails(IMessagesOptions options)
    {
        EmailGroupingCollection grouping = new EmailGroupingCollection();
        
        Email[]? emails = RetrieveEmails(options);
        emails.DetermineDomains();
        if (options.ShouldCacheEmails)
        {
            _cache.KeyDelete(options.Label);
            string emailsValue = JsonConvert.SerializeObject(emails);
            _cache.StringSet(options.Label, emailsValue);
        }

        if (emails == null)
        {
            throw new AggregateException("Could not retrieve emails");
        }

        var groupedEmails = emails
            .GroupBy(e => e.Domain)
            .OrderBy(g => g.Count());

        foreach (IGrouping<string?, Email> group in groupedEmails)
        {
            grouping.AddGrouping(new EmailGrouping(group));
        }

        return grouping;
    }

    public Label[] ListLabels()
    {
        var request = _service.Users.Labels.List("me");
        var response = request.ExecuteAsync().GetAwaiter().GetResult();
        if (response?.Labels == null)
        {
            throw new AggregateException("Could not return labels.");
        }

        return response.Labels.ToArray();
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

    private async Task<Email[]> FetchEmails(MessageBatch batch)
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

            Email email = new Email(response.Payload.Headers, id);
            emails.Add(email);
        }

        return emails.ToArray();
    }

    private void LoadMessages(ICollection<MessageBatch> messages, string pageToken, IMessagesOptions options)
    {
        if (string.IsNullOrEmpty(pageToken))
        {
            return;
        }

        var request = _service.Users.Messages.List("me");
        if (options.Label != "all")
        {
            request.LabelIds = options.Label?.ToUpper();
        }

        request.IncludeSpamTrash = false;
        request.MaxResults = options.MaxResults;
        request.PageToken = pageToken != "first" ? pageToken : null;

        if (options.Unread)
        {
            request.Q = "is:unread";
        }

        var response = request.ExecuteAsync().GetAwaiter().GetResult();
        if (response?.Messages == null)
        {
            throw new AggregateException("Could not return messages.");
        }

        messages.Add(new MessageBatch(response.Messages));
        LoadMessages(messages, response.NextPageToken, options);
    }

    private Email[]? RetrieveEmails(IMessagesOptions options)
    {
        if (options.ShouldGetCache)
        {
            Console.WriteLine("Fetching emails from a local cache. This shouldn't take long.");
            string storedEmailsJson = _cache.StringGet(options.Label)!;
            return JsonConvert.DeserializeObject<List<Email>>(storedEmailsJson)?.ToArray();
        }

        Console.WriteLine("Fetching message ids");
        List<MessageBatch> messageBatch = new List<MessageBatch>();
        LoadMessages(messageBatch, "first", options);

        Console.WriteLine("Fetching emails from server. This may take a while.");
        List<Task<Email[]>> tasks = new List<Task<Email[]>>();
        Parallel.ForEach(messageBatch, batch => { tasks.Add(FetchEmails(batch)); });
        var emailsTask = Task.WhenAll(tasks.ToArray()).GetAwaiter().GetResult();

        return emailsTask.SelectMany(t => t).ToArray();
    }

    #endregion

}
