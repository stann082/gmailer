using System.Windows.Input;
using core;
using service;

namespace ui.model;

public class PopupViewModel
{

    #region Constructos

    public PopupViewModel(IEmailService emailService, IList<EmailGrouping> selectionCache)
    {
        _emailService = emailService;
        _selectionCache = selectionCache;
        PopupAcceptCommand = new Command(PopupAccept);
    }

    #endregion

    #region Properties

    public ICommand PopupAcceptCommand { get; set; }

    #endregion

    #region Variables

    private readonly IEmailService _emailService;
    private readonly IList<EmailGrouping> _selectionCache;

    #endregion

    #region Helper Methods

    private void PopupAccept()
    {
        string response = _emailService.DeleteGroupings(_selectionCache.ToArray());
        if (string.IsNullOrEmpty(response))
        {
            
        }
    }

    #endregion

}
