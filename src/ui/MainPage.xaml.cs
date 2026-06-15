using core;
using service;
using Email = core.Email;
using Label = Google.Apis.Gmail.v1.Data.Label;

// The try/catch is taken care of in ExecuteOrWrap 
// ReSharper disable AsyncVoidMethod
// ReSharper disable AsyncVoidEventHandlerMethod

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
        await InvokeSafelyAsync(async () =>
        {
            base.OnAppearing();
            await _emailService.InitializeAsync();
            await LoadLabelsAsync();
            await UpdateLastSyncLabelAsync();
        });
    }

    private async void OnLabelChanged(object? sender, EventArgs e)
    {
        await InvokeSafelyAsync(async () =>
        {
            if (LabelPicker.SelectedItem is not Label selectedKey)
            {
                return;
            }

            _options.Label = selectedKey.Id;
            UpdateArchiveButtonState();
            await UpdateLastSyncLabelAsync();
            LoadEmailGroups();
        });
    }

    private async void OnSyncClicked(object? sender, EventArgs e)
    {
        await InvokeSafelyAsync(async () =>
        {
            if (LabelPicker.SelectedItem is not Label)
            {
                return;
            }

            _options.ShouldCacheEmails = true;
            _options.ShouldGetCache = false;

            await RunBatchTaskAsync(async progress =>
            {
                await _emailService.CacheEmailsAsync(new UiCacheOptions(), progress);
                var grouping = _emailService.ListEmails(_options);
                await MainThread.InvokeOnMainThreadAsync(() => { GroupPanel.SetItemsSource(grouping.Groupings); });
            }, "Syncing batch");

            _options.ShouldCacheEmails = false;
            _options.ShouldGetCache = true;

            await UpdateLastSyncLabelAsync();
        });
    }

    private async void OnArchiveClicked(object sender, EventArgs e)
    {
        await InvokeSafelyAsync(async () =>
        {
            Email[] selectedEmails = EmailPanel.GetSelectedEmails();

            if (selectedEmails.Length > 0)
            {
                await _emailService.ArchiveEmailsAsync(selectedEmails);
                if (ListOnlyCheckBox.IsChecked)
                {
                    LoadEmailGroups();
                    return;
                }
                if (GroupPanel.CurrentSelection is not { } currentGroup) return;
                foreach (var email in selectedEmails)
                {
                    currentGroup.Emails.Remove(email);
                }
            }
            else if (GroupPanel.CurrentSelection is { } group)
            {
                await _emailService.ArchiveGroupingsAsync([group]);
                if (GroupPanel.ItemsSource is { } groups)
                {
                    groups.Remove(group);
                }

                EmailPanel.SetItemsSource([]);
            }
        });
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        await InvokeSafelyAsync(async () =>
        {
            Email[] selectedEmails = EmailPanel.GetSelectedEmails();

            if (selectedEmails.Length > 0)
            {
                await _emailService.DeleteEmailsAsync(selectedEmails);
                if (ListOnlyCheckBox.IsChecked)
                {
                    LoadEmailGroups();
                    return;
                }
                if (GroupPanel.CurrentSelection is not { } currentGroup) return;
                foreach (var email in selectedEmails)
                {
                    currentGroup.Emails.Remove(email);
                }
            }
            else if (GroupPanel.CurrentSelection is { } group)
            {
                await _emailService.DeleteGroupingsAsync([group]);
                if (GroupPanel.ItemsSource is { } groups)
                {
                    groups.Remove(group);
                }

                EmailPanel.SetItemsSource([]);
            }
        });
    }

    private void OnGroupSelected(object? sender, EmailGrouping group)
    {
        var sorted = group.Emails.OrderByDescending(e =>
            DateTimeOffset.TryParse(e.Date, out var dt) ? dt : DateTimeOffset.MinValue);
        EmailPanel.SetItemsSource(sorted);
    }

    private void OnListOnlyChanged(object sender, CheckedChangedEventArgs e)
    {
        bool listOnly = e.Value;
        GroupPanel.IsVisible = !listOnly;
        MainGrid.ColumnDefinitions[0].Width = listOnly ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        LoadEmailGroups();
    }

    #endregion

    #region Helper Methods

    private async Task InvokeSafelyAsync(Func<Task> func, string errorTitle = "Unexpected Exception")
    {
        try
        {
            await func.Invoke();
        }
        catch (Exception ex)
        {
            await DisplayAlert(errorTitle, ex.Message, "OK");
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

    private void LoadEmailGroups()
    {
        EmailGroupingCollection grouping = _emailService.ListEmails(_options);
        if (ListOnlyCheckBox.IsChecked)
        {
            var allEmails = grouping.Groupings.SelectMany(g => g.Emails)
                .OrderByDescending(e => DateTimeOffset.TryParse(e.Date, out var dt) ? dt : DateTimeOffset.MinValue);
            EmailPanel.SetItemsSource(allEmails);
        }
        else
        {
            GroupPanel.SetItemsSource(grouping.Groupings);
            EmailPanel.SetItemsSource([]);
        }
    }

    private async Task LoadLabelsAsync()
    {
        await InvokeSafelyAsync(async () =>
        {
            Label[] labels = await _emailService.ListLabelsAsync();
            LabelPicker.ItemsSource = labels.ToList();
            if (labels.Length != 0)
            {
                int inboxIndex = Array.FindIndex(labels, l => l.Id == "INBOX");
                LabelPicker.SelectedIndex = inboxIndex >= 0 ? inboxIndex : 0;
            }
        }, "Error loading labels");
    }

    private void SetUiEnabled(bool enabled)
    {
        SyncBtn.IsEnabled = enabled;
        DeleteBtn.IsEnabled = enabled;
        ArchiveBtn.IsEnabled = enabled;
        GroupPanel.IsEnabled = enabled;
        EmailPanel.IsEnabled = enabled;

        if (enabled)
        {
            UpdateArchiveButtonState();
        }
    }

    private void UpdateArchiveButtonState()
    {
        ArchiveBtn.IsEnabled = LabelPicker.SelectedItem is Label { Id: not "ALL" };
    }

    private async Task UpdateLastSyncLabelAsync()
    {
        SyncState? lastSync = await _emailService.GetSyncStateAsync();
        LastSyncLabel.Text = lastSync?.LastSyncUtc is not null
            ? $"Last Sync: {lastSync.LastSyncUtc.Value.ToLocalTime():g}"
            : "Last Sync: Never";
    }

    #endregion
}
