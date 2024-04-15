using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Task<string> DeleteGroupings(IEnumerable<EmailGrouping> groupings);
    Task<EmailGroupingCollection> ListEmails(IMessagesOptions options);
    Task<IEnumerable<Label>> ListLabels();
    
}
