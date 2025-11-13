using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Task DeleteEmailsAsync(IEnumerable<Email> emails, string key);
    Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings, string key);
    Task<DateTime?> GetLastSyncAsync(string label);
    Task InitializeAsync();
    Task<EmailGroupingCollection> ListEmailsAsync(IMessagesOptions options, IProgress<(int current, int total)>? progress = null);
    Task<Label[]> ListLabelsAsync();
    Task SetLastSyncAsync(string label);

}
