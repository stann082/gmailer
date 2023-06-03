using service;

namespace ui;

public partial class MainPage : ContentPage
{

    #region Constructors

    public MainPage(IEmailService emailService)
    {
        _emailService = emailService;
        InitializeComponent();
    }

    #endregion

    #region Variables

    private readonly IEmailService _emailService;

    #endregion

    #region Event Handlers

    private void OnCounterClicked(object sender, EventArgs e)
    {
    }

    #endregion
        
}