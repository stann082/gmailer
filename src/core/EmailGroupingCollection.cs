namespace core;

public class EmailGroupingCollection
{

    #region Constructors

    public EmailGroupingCollection()
    {
        _groupings = new List<EmailGrouping>();
    }

    #endregion
    
    #region Variables

    private readonly List<EmailGrouping> _groupings;

    #endregion

    #region Public Methods

    public void AddGrouping(EmailGrouping grouping)
    {
        _groupings.Add(grouping);
    }

    public Email[] GetEmails()
    {
        return GetGroupings().SelectMany(g => g.Emails).ToArray();
    }
    
    public int GetEmailsTotal()
    {
        return GetEmails().Length;
    }
    
    public EmailGrouping[] GetGroupings()
    {
        return _groupings.ToArray();
    }

    #endregion

}
