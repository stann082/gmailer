using core;
using core.interfaces;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    
    Email[] ListEmails(IMessagesOptions options);
    Label[] ListLabels();
    
}
