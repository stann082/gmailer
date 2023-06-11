using core;
using service;

namespace ui.model;

public class MainPageViewModel
{

    #region Constructors

    public MainPageViewModel()
    {
        // a default constructor for xaml page binding
    }
    
    public MainPageViewModel(IEmailService emailService, IList<EmailGrouping> selectionCache)
    {
        EmailRepository = new EmailRepository();
        PopupViewModel = new PopupViewModel(emailService, selectionCache);
    }

    #endregion

    #region Properties

    public EmailRepository EmailRepository { get; }
    public PopupViewModel PopupViewModel { get; }

    #endregion

}
