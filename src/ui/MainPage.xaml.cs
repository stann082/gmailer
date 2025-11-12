using System.Collections.ObjectModel;
using core;
using service;
using Email = core.Email;

namespace ui;

public partial class MainPage
{
    #region Variables

    private readonly IEmailService _emailService;
    private readonly UiOptions _options;

    #endregion

    #region Constructors

    public MainPage(IEmailService emailService)
    {
        InitializeComponent();
        _emailService = emailService;
        _options = new UiOptions();

        GroupPanel.GroupSelected += OnGroupSelected;
    }

    #endregion

    #region Event Handlers

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            await _emailService.InitializeAsync();
            await LoadCacheKeysAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Exception", ex.Message, "OK");
        }
    }

    private async void OnCacheKeyChanged(object? sender, EventArgs e)
    {
        try
        {
            if (CacheKeyPicker.SelectedItem is not string selectedKey)
            {
                return;
            }

            _options.Label = selectedKey;
            await LoadGroupsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Exception", ex.Message, "OK");
        }
    }

    private async void OnSyncClicked(object? sender, EventArgs e)
    {
        try
        {
            _options.ShouldCacheEmails = true;
            _options.ShouldGetCache = false;

            await RunBatchTaskAsync(async progress =>
            {
                var grouping = await _emailService.ListEmailsAsync(_options, progress);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    GroupPanel.SetItemsSource(grouping.Groupings);
                });
            }, "Syncing batch");
            
            _options.ShouldCacheEmails = false;
            _options.ShouldGetCache = true;
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
            Email[] selectedEmails = EmailPanel.GetSelectedEmails();

            if (selectedEmails.Length > 0)
            {
                await _emailService.DeleteEmailsAsync(selectedEmails, _options.Label);
                if (GroupPanel.CurrentSelection is not EmailGrouping currentGroup) return;
                foreach (var email in selectedEmails)
                {
                    currentGroup.Emails.Remove(email);
                }
            }
            else if (GroupPanel.CurrentSelection is EmailGrouping group)
            {
                await _emailService.DeleteGroupingsAsync([group], _options.Label);
                if (GroupPanel.ItemsSource is ObservableCollection<EmailGrouping> groups)
                {
                    groups.Remove(group);
                }

                EmailPanel.SetItemsSource([]);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Exception", ex.Message, "OK");
        }
    }

    private void OnGroupSelected(object? sender, EmailGrouping group)
    {
        EmailPanel.SetItemsSource(group.Emails);
    }

    #endregion

    #region Helper Methods

    private async Task RunBatchTaskAsync(Func<IProgress<(int current, int total)>, Task> operation, string actionLabel = "Processing")
    {
        try
        {
            SetUiEnabled(false);
            ProgressOverlay.IsVisible = true;
            ProgressLabel.Text = $"{actionLabel}...";

            var progress = new Progress<(int current, int total)>(p =>
            {
                var total = Math.Max(1, p.total);
                var current = Math.Min(p.current, total);
                ProgressLabel.Text = $"{actionLabel} {current} of {total}...";
            });

            await operation(progress);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            ProgressOverlay.IsVisible = false;
            SetUiEnabled(true);
        }
    }

    private void SetUiEnabled(bool enabled)
    {
        SyncBtn.IsEnabled = enabled;
        DeleteBtn.IsEnabled = enabled;
        GroupPanel.IsEnabled = enabled;
        EmailPanel.IsEnabled = enabled;
    }

    private async Task LoadCacheKeysAsync()
    {
        try
        {
            // var keys = await _emailService.ListCacheKeysAsync();
            string[] keys = ["inbox", "all"];
            CacheKeyPicker.ItemsSource = keys.ToList();
            if (keys.Length != 0)
            {
                CacheKeyPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error loading cache keys", ex.Message, "OK");
        }
    }
    
    private async Task LoadGroupsAsync()
    {
        EmailGroupingCollection grouping = await _emailService.ListEmailsAsync(_options);
        GroupPanel.SetItemsSource(grouping.Groupings);
    }

    #endregion
}
