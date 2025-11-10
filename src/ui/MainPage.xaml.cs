using core;
using service;

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
            foreach (core.Email item in EmailList.SelectedItems)
            {
                // _db.KeyDelete($"email:{item.Id}");
            }
        }
        else if (GroupList.SelectedItem is EmailGrouping group)
        {
            // foreach (var item in group.Emails)
            //     _db.KeyDelete($"email:{item.Id}");
            //
            // _groups.Remove(group);
            // GroupList.ItemsSource = null;
            // GroupList.ItemsSource = _groups;
        }
    }

    private void OnGroupSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is EmailGrouping selected)
        {
            EmailList.ItemsSource = selected.Emails;
        }
    }
    
    #endregion
    
    private void LoadGroups()
    {
        UIOtions options = new UIOtions
        {
            Label = "inbox",
            ShouldGetCache = true
        };

        EmailGroupingCollection grouping = _emailService.ListEmails(options).GetAwaiter().GetResult();
        GroupList.ItemsSource = grouping.GetGroupings();
    }

}
