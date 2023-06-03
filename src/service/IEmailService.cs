using core;
using core.cli;
using Google.Apis.Gmail.v1.Data;

namespace service;

public interface IEmailService
{
    Email[] ListEmails(MessagesOptions options);
    Label[] ListLabels();
}
