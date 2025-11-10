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

public class EmailService(IConnectionMultiplexer redis) : IEmailService
{
    #region Variables

    private readonly IDatabase _cache = redis.GetDatabase();
    private GmailService? _service;

    #endregion

    #region Public Methods

    public async Task DeleteEmailAsync(string id)
    {
        if (_service == null)
        {
            Console.WriteLine("Gmail service is not initialized.");
            return;
        }
        
        var request = _service.Users.Messages.Delete("stanley.bennett@gmail.com", id);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            Console.WriteLine("Deleting a message...");
            await request.ExecuteAsync(cts.Token).ConfigureAwait(false);
            Console.WriteLine("Message successfully deleted...");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Request timed out.");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gmail delete failed: {ex}");
            throw;
        }
    }

    public async Task DeleteEmailsAsync(IEnumerable<Email> emails, string label)
    {
        if (_service == null)
        {
            Console.WriteLine("Gmail service is not initialized.");
            return;
        }
        
        BatchDeleteMessagesRequest messagesRequest = new BatchDeleteMessagesRequest { Ids = new List<string>() };
        string?[] emailIds = emails.Select(e => e.Id).ToArray();
        var idBatches = emailIds.Batch(1000);
        foreach (IEnumerable<string?> idBatch in idBatches)
        {
            messagesRequest.Ids.Clear();
            foreach (string? id in idBatch)
            {
                messagesRequest.Ids.Add(id);
            }

            var request = _service.Users.Messages.BatchDelete(messagesRequest, "me");
            await request.ExecuteAsync();
            string emailsValue = JsonConvert.SerializeObject(emails);
            _cache.StringSet(label, emailsValue);
        }
    }

    public async Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings, string label)
    {
        await DeleteEmailsAsync(groupings.SelectMany(g => g.Emails), label);
    }
    
    public async Task InitializeAsync()
    {
        var credential = await Authenticate();
        _service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Gmailer"
        });
    }

    public async Task<EmailGroupingCollection> ListEmailsAsync(IMessagesOptions options)
    {
        EmailGroupingCollection grouping = new EmailGroupingCollection();

        Email[]? emails = await RetrieveEmails(options);
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

        var groupedEmails = emails.GroupBy(e => e.Domain);
        groupedEmails = options.IsDescending
            ? groupedEmails.OrderByDescending(g => g.Count())
            : groupedEmails.OrderBy(g => g.Count());

        foreach (IGrouping<string?, Email> group in groupedEmails)
        {
            grouping.AddGrouping(new EmailGrouping(group));
        }

        return grouping;
    }

    public async Task<IEnumerable<Label>> ListLabelsAsync()
    {
        if (_service == null)
        {
            Console.WriteLine("Gmail service is not initialized.");
            return [];
        }
        
        var request = _service.Users.Labels.List("me");
        var response = await request.ExecuteAsync();
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
            [GmailService.Scope.MailGoogleCom],
            "user",
            CancellationToken.None,
            new FileDataStore("GmailAPI"));
        return credential;
    }

    private async Task<Email[]> FetchEmails(MessageBatch batch)
    {
        if (_service == null)
        {
            Console.WriteLine("Gmail service is not initialized.");
            return [];
        }
        
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

    private async Task LoadMessages(ICollection<MessageBatch> messages, string pageToken, IMessagesOptions options)
    {
        if (_service == null)
        {
            Console.WriteLine("Gmail service is not initialized.");
            return;
        }
        
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

        if (options.Recent > 0)
        {
            request.MaxResults = options.Recent;
        }
        else if (options.ResultsPePage > 0)
        {
            request.MaxResults = options.ResultsPePage;
        }
        else
        {
            request.MaxResults = 100;
        }

        request.PageToken = pageToken != "first" ? pageToken : null;

        if (options.Unread)
        {
            request.Q = "is:unread";
        }

        var response = await request.ExecuteAsync();
        if (response?.Messages == null)
        {
            throw new AggregateException("Could not return messages.");
        }

        messages.Add(new MessageBatch(response.Messages));
        if (options.Recent > 0)
        {
            return;
        }

        await LoadMessages(messages, response.NextPageToken, options);
    }

    private async Task<Email[]?> RetrieveEmails(IMessagesOptions options)
    {
        if (options.ShouldGetCache)
        {
            Console.WriteLine("Fetching emails from a local cache. This shouldn't take long.");
            string storedEmailsJson = _cache.StringGet(options.Label)!;
            return JsonConvert.DeserializeObject<List<Email>>(storedEmailsJson)?.ToArray();
        }

        Console.WriteLine("Fetching message ids");
        List<MessageBatch> messageBatches = new List<MessageBatch>();
        await LoadMessages(messageBatches, "first", options);

        List<Email> emails = new List<Email>();

        int batchCount = 1;
        foreach (MessageBatch messageBatch in messageBatches)
        {
            Console.Write($"\rProcessing {batchCount} out of {messageBatches.Count}");
            emails.AddRange(await FetchEmails(messageBatch));
            batchCount++;
        }

        Console.WriteLine();
        return emails.ToArray();
    }

    #endregion
}
