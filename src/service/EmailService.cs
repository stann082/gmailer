using core;
using core.interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MongoDB.Driver;
using Serilog;

namespace service;

public class EmailService(IMongoDatabase database) : IEmailService
{
    
    #region Variables

    private IMongoCollection<Email>? _emailsCollection;
    private IMongoCollection<string>? _syncCollection;
    private GmailService? _service;

    #endregion

    #region Public Methods

    public async Task DeleteEmailsAsync(IEnumerable<Email> emails, string key)
    {
        if (!IsValidConnection()) return;

        BatchDeleteMessagesRequest messagesRequest = new BatchDeleteMessagesRequest { Ids = new List<string>() };
        string?[] emailIds = emails.Select(e => e.EmailId).ToArray();
        var idBatches = emailIds.Batch(1000);
        foreach (IEnumerable<string?> idBatch in idBatches)
        {
            messagesRequest.Ids.Clear();
            foreach (string? id in idBatch)
            {
                messagesRequest.Ids.Add(id);
            }

            var request = _service!.Users.Messages.BatchDelete(messagesRequest, "me");
            await request.ExecuteAsync();
        }
        
        await UpdateEmailsCache(emailIds);
    }

    public async Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings, string key)
    {
        await DeleteEmailsAsync(groupings.SelectMany(g => g.Emails), key);
    }

    public async Task InitializeAsync()
    {
        var credential = await Authenticate();
        _service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Gmailer"
        });
        
        _emailsCollection = database.GetCollection<Email>("messages");
        _syncCollection = database.GetCollection<string>("sync_timestamps");
    }

    public Task<DateTime?> GetLastSyncAsync(string label)
    {
        string key = $"gmail:sync:last:{label}";
        // TODO: Fix caching
        // var raw = _cache.StringGet(key);
        // if (raw.IsNullOrEmpty)
        // {
        //     return Task.FromResult<DateTime?>(null);
        // }
        //
        // if (DateTime.TryParse(raw, out var dt))
        // {
        //     return Task.FromResult<DateTime?>(dt);
        // }

        return Task.FromResult<DateTime?>(null);
    }

    public async Task<EmailGroupingCollection> ListEmailsAsync(IMessagesOptions options, IProgress<(int current, int total)>? progress = null)
    {
        EmailGroupingCollection grouping = new EmailGroupingCollection();
        if (!IsValidConnection()) return grouping;

        Email[]? emails = await RetrieveEmails(options, progress);
        if (emails == null) return grouping;
        
        emails.DetermineDomains();

        if (options.ShouldCacheEmails)
        {
            await _emailsCollection.DeleteManyAsync(e => e.Label == options.Label);
            await _emailsCollection!.InsertManyAsync(emails);
        }

        if (emails == null)
        {
            throw new AggregateException("Could not retrieve emails");
        }

        var groupedEmails = emails.GroupBy(e => e.Domain).ToList();
        var singletons = groupedEmails.Where(g => g.Count() == 1).ToList();
        var normalGroups = groupedEmails.Where(g => g.Count() > 1);

        normalGroups = options.IsDescending
            ? normalGroups.OrderByDescending(g => g.Count())
            : normalGroups.OrderBy(g => g.Count());

        foreach (var group in normalGroups)
        {
            grouping.AddGrouping(new EmailGrouping(group));
        }

        if (singletons.Count > 0)
        {
            var miscEmails = singletons.SelectMany(g => g).ToList();
            var miscGroup = new EmailGrouping("misc", miscEmails);
            grouping.AddGrouping(miscGroup);
        }

        var resorted = options.IsDescending
            ? grouping.Groupings.OrderByDescending(g => g.Total).ToList()
            : grouping.Groupings.OrderBy(g => g.Total).ToList();

        grouping.Groupings.Clear();
        foreach (var group in resorted)
        {
            grouping.Groupings.Add(group);
        }

        return grouping;
    }

    public async Task<Label[]> ListLabelsAsync()
    {
        if (!IsValidConnection()) return [];

        var request = _service!.Users.Labels.List("me");
        var response = await request.ExecuteAsync();
        if (response?.Labels == null) throw new AggregateException("Could not return labels.");

        IList<Label> labels = response.Labels;
        labels = labels.Where(FilterEmailLabels).OrderBy(l => l.Type).ThenBy(l => l.Name).ToList();
        labels.Insert(0, new Label { Id = "ALL", Name = "ALL", Type = "system" });
        return labels.ToArray();
    }

    public async Task SetLastSyncAsync(string label)
    {
        var update = Builders<SyncState>.Update.Set(s => s.LastSyncUtc, DateTime.UtcNow);
        await _syncCollection.UpdateOneAsync(s => s.Label == label, update, new UpdateOptions { IsUpsert = true });
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

    private async Task<Email[]> FetchEmails(MessageBatch batch, IMessagesOptions options)
    {
        if (!IsValidConnection()) return [];

        List<Email> emails = new List<Email>();

        foreach (var id in batch.MessageIds)
        {
            var request = _service!.Users.Messages.Get("me", id);
            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
            var response = await request.ExecuteAsync();
            if (response == null)
            {
                throw new AggregateException("Could not return emails.");
            }

            Email email = new Email(response.Payload, options.Label, id, options.DoNotIncludeBody);
            emails.Add(email);
        }

        return emails.ToArray();
    }

    private static bool FilterEmailLabels(Label l)
    {
        return !l.Id.StartsWith("CATEGORY_") && l.Id != "CHAT" && l.Id != "DRAFT"
               && l.Id != "IMPORTANT" && l.Id != "UNREAD" && l.Id != "STARRED" && l.Id != "YELLOW_STAR";
    }

    private Email[]? GetCachedEmails(string label)
    {
        var emails = _emailsCollection.Find(x => x.Label == label).ToList();
        return emails.Count != 0 ? emails.ToArray() : null;
    }

    private bool IsValidConnection()
    {
        if (_service == null)
        {
            Log.Error("Gmail service is not initialized");
            return false;
        }

        if (_emailsCollection == null)
        {
            Log.Error("Mongo DB is not initialized");
            return false;
        }

        return true;
    }

    private async Task LoadMessages(ICollection<MessageBatch> messages, string pageToken, IMessagesOptions options)
    {
        if (!IsValidConnection()) return;

        if (string.IsNullOrEmpty(pageToken))
        {
            return;
        }

        var request = _service!.Users.Messages.List("me");
        if (options.Label != "ALL")
        {
            request.LabelIds = options.Label;
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

    private async Task<Email[]?> RetrieveEmails(IMessagesOptions options, IProgress<(int current, int total)>? progress = null)
    {
        if (options.ShouldGetCache) return GetCachedEmails(options.Label);

        var messageBatches = new List<MessageBatch>();
        await LoadMessages(messageBatches, "first", options);

        var emails = new List<Email>();
        int total = messageBatches.Count;
        int current = 0;

        progress?.Report((0, total));
        foreach (var batch in messageBatches)
        {
            current++;
            Log.Information("\rProcessing {Current} out of {MessageBatchCount}", current, messageBatches.Count);
            progress?.Report((current, total));
            emails.AddRange(await FetchEmails(batch, options));
        }

        return emails.ToArray();
    }

    private async Task UpdateEmailsCache(string?[] emailIds)
    {
        if (emailIds.Length == 0) return;
        var filter = Builders<Email>.Filter.And(Builders<Email>.Filter.In(e => e.EmailId, emailIds));
        await _emailsCollection!.DeleteManyAsync(filter);
    }

    #endregion
}
