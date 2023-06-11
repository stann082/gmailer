using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    string DeleteGroupings(IEnumerable<EmailGrouping> groupings);
    EmailGroupingCollection ListEmails(IMessagesOptions options);
    IEnumerable<Label> ListLabels();
    
}
