using core;
using service;
using Email = core.Email;

namespace ui;

public partial class MainPage
{

    #region Constructors

    public MainPage(IEmailService emailService)
    {
        InitializeComponent();
        _emailService = emailService;
        LoadGroups();
    }

    #endregion

    #region Variables

    private readonly IEmailService _emailService;

    #endregion

    #region Event Handlers

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        if (EmailList.SelectedItems?.Count > 0)
        {
            Email[]? emails = EmailList.SelectedItems as Email[];
            if (emails == null) return;
            _emailService.DeleteEmailsAsync(emails).GetAwaiter().GetResult();
        }
        else if (GroupList.SelectedItem is EmailGrouping group)
        {
            _emailService.DeleteGroupingsAsync([group]).GetAwaiter().GetResult();
        }
    }

    private void OnGroupSelected(object? sender, SelectionChangedEventArgs e)
    {
        EmailList.SelectedItems = [];
        if (e.CurrentSelection.FirstOrDefault() is EmailGrouping selected)
        {
            EmailList.ItemsSource = selected.Emails;
        }
    }
    
    #endregion

    #region Helper Methods

    private void LoadGroups()
    {
        UIOtions options = new UIOtions
        {
            Label = "inbox",
            ShouldGetCache = true
        };

        EmailGroupingCollection grouping = _emailService.ListEmailsAsync(options).GetAwaiter().GetResult();
        GroupList.ItemsSource = grouping.GetGroupings();
    }

    #endregion

}
