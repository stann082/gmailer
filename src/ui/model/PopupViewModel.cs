using System.ComponentModel;
using System.Windows.Input;
using service;

namespace ui.model;

public class PopupViewModel : INotifyPropertyChanged
{

    #region Constructos

    public PopupViewModel(IEmailService emailService)
    {
        _emailService = emailService;
        PopupAcceptCommand = new Command(PopupAccept); //CanExecute() will be call the PopupAccept method
        PopupCommand = new Command(Popup);
    }

    #endregion

    #region Properties

    public ICommand PopupAcceptCommand { get; set; }
    public ICommand PopupCommand { get; set; }

    public bool PopupOpen
    {
        get => isOpen;
        set
        {
            isOpen = value;
            OnPropertyChanged(nameof(PopupOpen));
        }
    }

    #endregion
    
    #region Variables

    private IEmailService _emailService;
    private bool isOpen;

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region Helper Methods

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Popup()
    {
        PopupOpen = true;
    }

    private void PopupAccept()
    {
        // You can write your set of codes that needs to be executed.
    }

    #endregion
    
}
