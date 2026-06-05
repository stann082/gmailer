using System.Text;
using Google.Apis.Gmail.v1.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace core;

public class Email
{
    #region Constructors

    public Email()
    {
        // for json deserialization
    }

    public Email(Message message, string id, bool doNotIncludeBody)
    {
        Id = id;
        Labels = message.LabelIds;
        Initialize(message.Payload, doNotIncludeBody);
    }

    #endregion

    #region Properties

    [BsonId]
    public string Id { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Date { get; private set; } = string.Empty;
    public string Domain { get; private set; } = string.Empty;
    public IList<string>? Labels { get; private set; }
    public string Name { get; set; } = string.Empty;
    public string Sender { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;

    #endregion

    #region Helper Methods

    private static string DecodeBase64Url(string input)
    {
        string base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string GetBodyFromParts(IList<MessagePart>? parts)
    {
        if (parts == null)
        {
            return string.Empty;
        }

        foreach (var part in parts)
        {
            switch (part)
            {
                case { MimeType: "text/html", Body: not null } when !string.IsNullOrEmpty(part.Body.Data):
                case { MimeType: "text/plain", Body: not null } when !string.IsNullOrEmpty(part.Body.Data):
                {
                    return DecodeBase64Url(part.Body.Data);
                }
            }

            if (part.Parts is not { Count: > 0 })
            {
                continue;
            }

            var result = GetBodyFromParts(part.Parts);
            if (string.IsNullOrEmpty(result))
            {
                continue;
            }

            return result;
        }

        return string.Empty;
    }

    private void Initialize(MessagePart payload, bool doNotIncludeBody)
    {
        if (!doNotIncludeBody)
        {
            Body = GetBodyFromParts(payload.Parts);
        }

        foreach (var header in payload.Headers)
        {
            switch (header.Name)
            {
                case "Date":
                    Date = header.Value;
                    break;
                case "From":
                    Sender = header.Value;
                    SetSenderProperties(Sender);
                    break;
                case "Subject":
                    Subject = header.Value;
                    break;
            }
        }
        
        SetDomain();
    }

    private void SetDomain()
    {
        if (string.IsNullOrEmpty(Sender))
        {
            return;
        }

        if (!Sender.Contains('@'))
        {
            Domain = Sender;
            return;
        }

        string[] recipientSplit = Sender.Split('@');
        string[]? domainParts = recipientSplit.LastOrDefault()?.Split('.');
        string? lastTwoParts = domainParts?.Length >= 2 ? string.Join('.', domainParts, domainParts.Length - 2, 2) : recipientSplit.LastOrDefault();
        Domain = lastTwoParts?.Trim('<', '>');
    }

    private void SetSenderProperties(string sender)
    {
        int startIndex = sender.IndexOf('<');
        int endIndex = sender.LastIndexOf('>');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            Name = sender[..startIndex].Trim();
            Address = sender.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
        }
        else
        {
            Name = string.Empty;
            Address = sender.Trim();
        }

        Name = Name.Trim('"');
        Address = Address.Trim('"');
    }

    #endregion
}
