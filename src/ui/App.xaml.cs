namespace ui;

public partial class App : Application
{

    #region Constructors

    public App(MainPage mainPage)
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    #endregion

}
