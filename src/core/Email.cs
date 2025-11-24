using System.Text;
using Google.Apis.Gmail.v1.Data;

namespace core;

public class Email
{

    #region Constructors

    public Email()
    {
        // for json deserialization
    }

    public Email(MessagePart payload, string emailId, string label, bool doNotIncludeBody)
    {
        EmailId = emailId;
        Label = label;
        Initialize(payload, doNotIncludeBody);
    }

    #endregion

    #region Properties

    public string? Address { get; private set; } = string.Empty;
    public string? Body { get; set; } = string.Empty;
    public string? Date { get; private set; } = string.Empty;
    public string? Domain { get; set; } = string.Empty;
    public string? EmailId { get; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? Sender { get; private set; } = string.Empty;
    public string? Subject { get; private set; } = string.Empty;

    #endregion

    #region Public Methods

    public DateTime ToDateTime()
    {
        return DateTime.TryParse(Date, out var result) ? result : DateTime.MinValue;
    }

    #endregion
    
    #region Helper Methods

    private static string DecodeBase64Url(string input)
    {
        string base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "=";  break;
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
    
    private static string? GetBodyFromParts(IList<MessagePart>? parts)
    {
        if (parts == null)
        {
            return null;
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

        return null;
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
