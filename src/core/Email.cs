using Google.Apis.Gmail.v1.Data;

namespace core;

public class Email
{

    #region Constructors

    public Email(IEnumerable<MessagePartHeader> headers)
    {
        Initialize(headers);
    }

    #endregion

    #region Properties

    public string? Date { get; set; }
    public string? From { get; set; }
    public string? Subject { get; set; }

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
                    From = header.Value;
                    break;
                case "Subject":
                    Subject = header.Value;
                    break;
            }
        }
    }

    #endregion

}
