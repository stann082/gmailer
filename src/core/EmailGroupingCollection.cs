using System.Collections.ObjectModel;

namespace core;

public class EmailGroupingCollection
{

    #region Properties

    public ObservableCollection<EmailGrouping> Groupings { get; } = new();

    #endregion
    
    #region Public Methods

    public void AddGrouping(EmailGrouping grouping)
    {
        Groupings.Add(grouping);
    }

    public Email[] GetEmails()
    {
        return Groupings.SelectMany(g => g.Emails).ToArray();
    }

    public int GetEmailsTotal()
    {
        return Groupings.Sum(g => g.Emails.Count);
    }

    #endregion

}
