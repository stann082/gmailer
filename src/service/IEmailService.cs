using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Task CacheEmailsAsync(ICacheOptions cacheOptions, IProgress<(int current, int total)>? progress = null);
    Task ClearEmailsCache();
    Task DeleteEmailsAsync(IEnumerable<Email> emails);
    Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings);
    Task<SyncState?> GetSyncStateAsync();
    Task InitializeAsync();
    EmailGroupingCollection ListEmails(IMessagesOptions messagesOptions);
    Task<Label[]> ListLabelsAsync();

}
