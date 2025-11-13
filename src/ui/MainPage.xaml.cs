using core;
using service;
using Email = core.Email;
using Label = Google.Apis.Gmail.v1.Data.Label;
// ReSharper disable AsyncVoidMethod - try/catch is taken care of in ExecuteOrWrap

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
        await ExecuteOrWrap(async () =>
        {
            base.OnAppearing();
            await _emailService.InitializeAsync();
            await LoadLabelsAsync();
            await UpdateLastSyncLabelAsync();
        });
    }

    private async void OnLabelChanged(object? sender, EventArgs e)
    {
        try
        {
            if (LabelPicker.SelectedItem is not Label selectedKey)
            {
                return;
            }

            _options.Label = selectedKey.Id;
            await UpdateLastSyncLabelAsync();
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
            if (LabelPicker.SelectedItem is not Label selectedLabel)
            {
                return;
            }

            _options.Label = selectedLabel.Id;
            _options.ShouldCacheEmails = true;
            _options.ShouldGetCache = false;

            await RunBatchTaskAsync(async progress =>
            {
                var grouping = await _emailService.ListEmailsAsync(_options, progress);
                await MainThread.InvokeOnMainThreadAsync(() => { GroupPanel.SetItemsSource(grouping.Groupings); });
            }, "Syncing batch");

            _options.ShouldCacheEmails = false;
            _options.ShouldGetCache = true;

            await _emailService.SetLastSyncAsync(_options.Label);
            await UpdateLastSyncLabelAsync();
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

            string key = _options.GetCacheKey();
            if (selectedEmails.Length > 0)
            {
                await _emailService.DeleteEmailsAsync(selectedEmails, key);
                if (GroupPanel.CurrentSelection is not { } currentGroup) return;
                foreach (var email in selectedEmails)
                {
                    currentGroup.Emails.Remove(email);
                }
            }
            else if (GroupPanel.CurrentSelection is { } group)
            {
                await _emailService.DeleteGroupingsAsync([group], key);
                if (GroupPanel.ItemsSource is { } groups)
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

    private async Task ExecuteOrWrap(Func<Task> func)
    {
        try
        {
            await func.Invoke();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Exception", ex.Message, "OK");
        }
    }

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

    private async Task LoadLabelsAsync()
    {
        try
        {
            Label[] labels = await _emailService.ListLabelsAsync();
            LabelPicker.ItemsSource = labels.ToList();
            if (labels.Length != 0)
            {
                LabelPicker.SelectedIndex = 0;
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

    private async Task UpdateLastSyncLabelAsync()
    {
        var lastSync = await _emailService.GetLastSyncAsync(_options.Label);
        LastSyncLabel.Text = lastSync is null ? "Last Sync: Never" : $"Last Sync: {lastSync.Value.ToLocalTime():g}";
    }

    #endregion
}
