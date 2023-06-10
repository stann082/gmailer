using System.Collections.ObjectModel;

namespace ui.model;

public class EmailModel
{
    public ObservableCollection<core.Email> Emails { get; set; }

    public EmailModel()
    {
        Emails = new ObservableCollection<core.Email>();
    }

}
