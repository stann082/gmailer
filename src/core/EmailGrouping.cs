namespace core;

public class EmailGrouping
{

    #region Constructors

    public EmailGrouping(IGrouping<string?, Email> group)
    {
        Id = Guid.NewGuid().ToString();
        Emails = group.ToArray();
        Domain = group.Key;
        Total = group.Count();
    }

    public EmailGrouping(Email[] emails)
    {
        Id = Guid.NewGuid().ToString();
        Emails = emails;
        Total = emails.Length;
    }

    #endregion

    #region Properties

    public string? Domain { get; }
    public Email[] Emails { get; }
    public string Id { get; }
    public int Total { get; }

    #endregion

    #region Public Methods

    public string? GetName()
    {
        Email? email = Emails.FirstOrDefault();
        if (email == null)
        {
            return string.Empty;
        }

        return !string.IsNullOrEmpty(email.Name) ? email.Name : email.Address;
    }

    #endregion

}
