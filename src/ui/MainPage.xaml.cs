using System.Collections.ObjectModel;
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
        _options = new UiOptions();
        _emailService = emailService;
    }

    #endregion

    #region Variables

    private readonly IEmailService _emailService;
    private readonly UiOptions _options;

    #endregion

    #region Event Handlers

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _emailService.InitializeAsync();
            await LoadGroupsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Exception", ex.Message, "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        try
        {
            if (EmailList.SelectedItems?.Count > 0)
            {
                Email[] emails = EmailList.SelectedItems.Cast<Email>().ToArray();
                await _emailService.DeleteEmailsAsync(emails, _options.Label);
                if (GroupList.SelectedItem is not EmailGrouping currentGroup) return;
                foreach (var email in emails)
                {
                    currentGroup.Emails.Remove(email);
                }
            }
            else if (GroupList.SelectedItem is EmailGrouping group)
            {
                await _emailService.DeleteGroupingsAsync([group], _options.Label);
                if (GroupList.ItemsSource is ObservableCollection<EmailGrouping> groups)
                {
                    groups.Remove(group);
                }

                EmailList.ItemsSource = null;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Exception", ex.Message, "OK");
        }
    }

    private void OnGroupSelected(object? sender, SelectionChangedEventArgs e)
    {
        EmailList.SelectedItems = [];
        if (e.CurrentSelection.FirstOrDefault() is not EmailGrouping selected) return;
        EmailList.ItemsSource = selected.Emails;
    }

    #endregion

    #region Helper Methods

    private async Task LoadGroupsAsync()
    {
        EmailGroupingCollection grouping = await _emailService.ListEmailsAsync(_options);
        GroupList.ItemsSource = grouping.Groupings;
    }

    #endregion
}
