namespace ui.Views;
using Email = core.Email;

public partial class EmailListView
{
    public EmailListView()
    {
        InitializeComponent();
    }

    public void SetItemsSource(IEnumerable<Email> emails)
    {
        EmailList.ItemsSource = emails;
    }

    public Email[] GetSelectedEmails()
    {
        return EmailList.SelectedItems?.Cast<Email>().ToArray() ?? [];
    }
}
