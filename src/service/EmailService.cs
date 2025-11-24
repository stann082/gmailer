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
    private IMongoCollection<SyncState>? _syncCollection;
    private GmailService? _service;

    #endregion

    #region Public Methods

    public async Task CacheEmailsAsync(ICacheOptions cacheOptions, IProgress<(int current, int total)>? progress = null)
    {
        if (!IsValidConnection()) return;

        Email[]? emails = await FetchEmailsFromServer(cacheOptions, progress);
        if (emails == null) return;
        emails.DetermineDomains();

        await _emailsCollection!.DeleteManyAsync(FilterDefinition<Email>.Empty);
        await InsertEmailsAsync(emails);
        await SetLastSyncAsync();
    }

    public async Task DeleteEmailsAsync(IEnumerable<Email> emails)
    {
        if (!IsValidConnection()) return;

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

            var request = _service!.Users.Messages.BatchDelete(messagesRequest, "me");
            await request.ExecuteAsync();
        }

        await UpdateEmailsCache(emailIds);
    }

    public async Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings)
    {
        await DeleteEmailsAsync(groupings.SelectMany(g => g.Emails));
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
        _syncCollection = database.GetCollection<SyncState>("sync_timestamps");
    }

    public async Task<DateTime?> GetLastSyncAsync()
    {
        var state = await _syncCollection.Find(s => s.Id == Constants.SyncTimestampId).FirstOrDefaultAsync();
        return state?.LastSyncUtc;
    }

    public EmailGroupingCollection ListEmails(IMessagesOptions messagesOptions)
    {
        EmailGroupingCollection grouping = new EmailGroupingCollection();
        if (!IsValidConnection()) return grouping;

        Email[]? emails = FetchEmailsFromCache(messagesOptions.Label);
        if (emails == null) return grouping;
        emails.DetermineDomains();

        var groupedEmails = emails.GroupBy(e => e.Domain).ToList();
        var singletons = groupedEmails.Where(g => g.Count() == 1).ToList();
        var normalGroups = groupedEmails.Where(g => g.Count() > 1);

        normalGroups = messagesOptions.IsDescending
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

        var resorted = messagesOptions.IsDescending
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

    public async Task SetLastSyncAsync()
    {
        var update = Builders<SyncState>.Update.Set(s => s.LastSyncUtc, DateTime.UtcNow);
        await _syncCollection.UpdateOneAsync(
            s => s.Id == Constants.SyncTimestampId,
            update,
            new UpdateOptions { IsUpsert = true }
        );
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
            new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            [GmailService.Scope.MailGoogleCom],
            "user",
            CancellationToken.None,
            new FileDataStore("GmailAPI"));
        return credential;
    }

    private Email[]? FetchEmailsFromCache(string label)
    {
        var emails = _emailsCollection.Find(x => x.Labels.Contains(label)).ToList();
        return emails.Count != 0 ? emails.ToArray() : null;
    }

    private async Task<Email[]?> FetchEmailsFromServer(ICacheOptions cacheOptions, IProgress<(int current, int total)>? progress = null)
    {
        if (!IsValidConnection()) return [];

        var messageBatches = new List<MessageBatch>();
        await LoadMessages(messageBatches, "first");

        var allEmails = new List<Email>();
        int total = messageBatches.Count;
        int current = 0;

        progress?.Report((0, total));
        foreach (var batch in messageBatches)
        {
            current++;
            Log.Information("\rProcessing {Current} out of {MessageBatchCount}", current, messageBatches.Count);
            progress?.Report((current, total));
            List<Email> emailsBatch = [];

            foreach (string id in batch.MessageIds)
            {
                var request = _service!.Users.Messages.Get("me", id);
                request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                var response = await request.ExecuteAsync();
                if (response == null)
                {
                    throw new AggregateException("Could not return emails.");
                }

                Email email = new Email(response, id, cacheOptions.DoNotIncludeBody);
                emailsBatch.Add(email);
            }

            allEmails.AddRange(emailsBatch);
        }

        return allEmails.ToArray();
    }

    private static bool FilterEmailLabels(Label label)
    {
        return !label.Id.StartsWith("CATEGORY_") && label.Id != "CHAT" && label.Id != "DRAFT"
               && label.Id != "IMPORTANT" && label.Id != "UNREAD" && label.Id != "STARRED" && label.Id != "YELLOW_STAR";
    }

    private async Task InsertEmailsAsync(Email[] emails)
    {
        foreach (var email in emails.Where(e => !string.IsNullOrWhiteSpace(e.Id)))
        {
            var filter = Builders<Email>.Filter.Eq(e => e.Id, email.Id);
            await _emailsCollection!.ReplaceOneAsync(
                filter,
                email,
                new ReplaceOptions { IsUpsert = true }
            );
        }
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

    private async Task LoadMessages(ICollection<MessageBatch> messages, string pageToken)
    {
        if (!IsValidConnection()) return;

        while (true)
        {
            if (string.IsNullOrEmpty(pageToken))
            {
                return;
            }

            var request = _service!.Users.Messages.List("me");
            request.IncludeSpamTrash = false;
            request.PageToken = pageToken != "first" ? pageToken : null;

            ListMessagesResponse? response;
            try
            {
                response = await request.ExecuteAsync();
                if (response?.Messages == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching messages from the Gmail server");
                return;
            }

            messages.Add(new MessageBatch(response.Messages));
            pageToken = response.NextPageToken;
        }
    }

    private async Task UpdateEmailsCache(string?[] emailIds)
    {
        if (emailIds.Length == 0) return;
        var filter = Builders<Email>.Filter.And(Builders<Email>.Filter.In(e => e.Id, emailIds));
        await _emailsCollection!.DeleteManyAsync(filter);
    }

    #endregion
    
}
