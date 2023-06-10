using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    EmailGroupingCollection ListEmails(IMessagesOptions options);
    Label[] ListLabels();
    
}
