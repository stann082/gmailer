using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Task CacheEmailsAsync(ICacheOptions cacheOptions, IProgress<(int current, int total)>? progress = null);
    Task DeleteEmailsAsync(IEnumerable<Email> emails);
    Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings);
    Task<DateTime?> GetLastSyncAsync();
    Task InitializeAsync();
    EmailGroupingCollection ListEmails(IMessagesOptions messagesOptions, IProgress<(int current, int total)>? progress = null);
    Task<Label[]> ListLabelsAsync();
    Task SetLastSyncAsync();

}
