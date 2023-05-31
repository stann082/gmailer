using core.cli;

namespace service;

public interface IEmailService
{
    Task ListEmails(ListOptions options);
}
