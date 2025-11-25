using Serilog;

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

    private async void OnEmailDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (e.Parameter is not Email email)
            {
                return;
            }

            await Shell.Current.Navigation.PushAsync(new EmailDetailPage(email));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error displaying email detail");
        }
    }
}
