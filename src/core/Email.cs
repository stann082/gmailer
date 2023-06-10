using Google.Apis.Gmail.v1.Data;

namespace core;

public class Email
{

    #region Constructors

    public Email()
    {
        // for json deserialization
    }

    public Email(IEnumerable<MessagePartHeader> headers, string id)
    {
        Id = id;
        Initialize(headers);
    }

    #endregion

    #region Properties

    public string? Address { get; set; }
    public string? Date { get; set; }
    public string? Domain { get; set; }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Sender { get; set; }
    public string? Subject { get; set; }

    #endregion

    #region Public Methods

    public DateTime ToDateTime()
    {
        return DateTime.TryParse(Date, out var result) ? result : DateTime.MinValue;
    }

    #endregion
    
    #region Helper Methods

    private void Initialize(IEnumerable<MessagePartHeader> headers)
    {
        foreach (var header in headers)
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
