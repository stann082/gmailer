using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Task DeleteEmailsAsync(IEnumerable<Email> emails);
    Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings);
    Task<DateTime?> GetLastSyncAsync();
    Task InitializeAsync();
    Task<EmailGroupingCollection> ListEmailsAsync(IMessagesOptions options, IProgress<(int current, int total)>? progress = null);
    Task<Label[]> ListLabelsAsync();
    Task SetLastSyncAsync();

}
