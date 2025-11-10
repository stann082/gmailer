using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Task DeleteEmailsAsync(IEnumerable<Email> emails);
    Task DeleteGroupingsAsync(IEnumerable<EmailGrouping> groupings);
    Task<EmailGroupingCollection> ListEmailsAsync(IMessagesOptions options);
    Task<IEnumerable<Label>> ListLabelsAsync();
    
}
