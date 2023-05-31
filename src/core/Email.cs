using Google.Apis.Gmail.v1.Data;

namespace core;

public class Email
{

    #region Constructors

    public Email(IEnumerable<MessagePartHeader> headers, string id)
    {
        Id = id;
        Initialize(headers);
    }

    #endregion

    #region Properties

    public string? Address { get; private set; }
    public string? Date { get; private set; }
    public string? Domain { get; set; }

    public string? Id { get; private set; }
    public string? Name { get; private set; }
    public string? Sender { get; private set; }
    public string? Subject { get; private set; }

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
