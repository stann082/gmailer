using Email = core.Email;

namespace ui.Views;

public partial class EmailDetailPage : ContentPage
{
    public EmailDetailPage(Email email)
    {
        InitializeComponent();
        BindingContext = email;
    }
}
