using System.Collections.ObjectModel;

namespace ui.model;

public class EmailRepository
{

    #region Constructors

    public EmailRepository()
    {
        Emails = new ObservableCollection<core.EmailGrouping>();
    }

    #endregion

    #region Properties

    public ObservableCollection<core.EmailGrouping> Emails { get; set; }

    #endregion

}
