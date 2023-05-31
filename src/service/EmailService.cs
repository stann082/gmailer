using System.Text;
using core;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace service;

public class EmailService
{

    #region Public Methods

    public async Task GetEmails()
    {
        string? clientId = Environment.GetEnvironmentVariable("GMAIL_CLIENT_ID");
        string? clientSecret = Environment.GetEnvironmentVariable("GMAIL_CLIENT_SECRET");
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new AggregateException("Client id or client secret aren't set");
        }

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            new[] { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify },
            "user",
            CancellationToken.None,
            new FileDataStore("etc"));

        // Create the service.
        var service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Gmailer"
        });

        var emailListRequest = service.Users.Messages.List("me");
        emailListRequest.LabelIds = "INBOX";
        emailListRequest.IncludeSpamTrash = false;
        //emailListRequest.Q = "is:unread"; // For unread emails

        var emailListResponse = await emailListRequest.ExecuteAsync();
        if (emailListResponse?.Messages == null)
        {
            return;
        }

        List<Email> emails = new List<Email>();
        foreach (var email in emailListResponse.Messages)
        {
            var emailInfoRequest = service.Users.Messages.Get("me", email.Id);
            var emailInfoResponse = await emailInfoRequest.ExecuteAsync();
            if (emailInfoResponse == null)
            {
                continue;
            }

            emails.Add(new Email(emailInfoResponse.Payload.Headers));
        }
        
        Console.WriteLine($"All emails {emails.Count}");
    }

    #endregion

}
