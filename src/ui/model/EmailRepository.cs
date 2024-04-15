using System.Collections.ObjectModel;
using Email = core.Email;

namespace ui.model;

public class EmailRepository
{

    #region Constructors

    public EmailRepository()
    {
        EmailGroups = new ObservableCollection<core.EmailGrouping>();
        Emails = new ObservableCollection<Email>();
    }

    #endregion

    #region Properties

    public ObservableCollection<core.EmailGrouping> EmailGroups { get; set; }
    public ObservableCollection<Email> Emails { get; set; }

    #endregion

}
