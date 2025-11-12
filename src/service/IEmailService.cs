using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Task DeleteEmailsAsync(IEnumerable<Email> emails, string label);
    Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings, string label);
    Task InitializeAsync();
    Task<EmailGroupingCollection> ListEmailsAsync(IMessagesOptions options, IProgress<(int current, int total)>? progress = null);
    Task<IEnumerable<Label>> ListLabelsAsync();
    
}
