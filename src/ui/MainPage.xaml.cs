using core;
using core.interfaces;
using core.nullobj;
using service;
using Syncfusion.Maui.ListView;
using ui.model;
using Email = core.Email;

namespace ui;

public partial class MainPage
{

    #region Constructors

    public MainPage(IEmailService emailService)
    {
        InitializeComponent();

        _selectionCache = new List<EmailGrouping>();
        _viewModel = new EmailRepository();

        BindingContext = new MainPageViewModel(emailService, _selectionCache);
        btnDelete.IsEnabled = false;
        lblSelectionTotal.Text = "Total emails selected: 0";

        MessagesOptions options = new MessagesOptions();
        options.ShouldGetCache = true;
        options.Label = "all";
        options.ResultsPePage = 100;

        InitializeEmailRepository(emailService, options).Wait();
        InitializeGroupingListView();
        InitializeEmailListView();
    }

    #endregion

    #region Variables

    private readonly IList<EmailGrouping> _selectionCache;
    private readonly EmailRepository _viewModel;

    #endregion

    #region Event Handlers

    private void Popup_Clicked(object sender, EventArgs e)
    {
        if (groupingListView.SelectedItems == null)
        {
            return;
        }

        foreach (var selectedItem in groupingListView.SelectedItems)
        {
            if (selectedItem is not EmailGrouping item)
            {
                continue;
            }

            _selectionCache.Add(item);
        }

        popup.Show();
    }

    private void ListView_OnSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
    {
        if (groupingListView.SelectedItems == null)
        {
            return;
        }

        btnDelete.IsEnabled = groupingListView.SelectedItems.Any();

        EmailGrouping[] emailGroupings = groupingListView.SelectedItems.Cast<EmailGrouping>().ToArray();
        int totalEmails = emailGroupings.Sum(grouping => grouping.Total);
        lblSelectionTotal.Text = $"Total emails selected: {totalEmails}";

        if (!emailGroupings.Any())
        {
            _viewModel.Emails.Clear();
            return;
        }
        
        foreach (Email email in emailGroupings.SelectMany(g => g.Emails))
        {
            _viewModel.Emails.Add(email);
        }
    }

    #endregion

    #region Helper Methods

    private async Task InitializeEmailRepository(IEmailService emailService, IMessagesOptions options)
    {
        EmailGroupingCollection grouping = await emailService.ListEmails(options);
        foreach (var email in grouping.GetGroupings().OrderByDescending(g => g.Total))
        {
            _viewModel.EmailGroups.Add(email);
        }
    }

    private void InitializeEmailListView()
    {
        emailListView.ItemsSource = _viewModel.Emails;
        emailListView.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            var name = new Label { FontAttributes = FontAttributes.Bold, TextColor = Colors.Black, FontSize = 18 };
            name.SetBinding(Label.TextProperty, new Binding("Name"));
            var subject = new Label { TextColor = Colors.Gray, FontSize = 12 };
            subject.SetBinding(Label.TextProperty, new Binding("Subject"));

            grid.Children.Add(name);
            grid.Children.Add(subject);
            grid.SetRow(name, 0);
            grid.SetRow(subject, 1);

            return grid;
        });
    }

    private void InitializeGroupingListView()
    {
        groupingListView.ItemsSource = _viewModel.EmailGroups;
        groupingListView.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            var domain = new Label { FontAttributes = FontAttributes.Bold, TextColor = Colors.Black, FontSize = 21 };
            domain.SetBinding(Label.TextProperty, new Binding("Domain"));
            var total = new Label { TextColor = Colors.Gray, FontSize = 15 };
            total.SetBinding(Label.TextProperty, new Binding("Total"));

            grid.Children.Add(domain);
            grid.Children.Add(total);
            grid.SetRow(domain, 0);
            grid.SetRow(total, 1);

            return grid;
        });
    }

    #endregion

    #region Helper Classes

    // TODO: bind IMessagesOptions to the UI controls
    private class MessagesOptions : NullMessagesOptions
    {

        #region Properties

        public override string Label { get; set; }
        public override int ResultsPePage { get; set; }
        public override int Recent { get; set; }
        public override bool ShouldGetCache { get; set; }

        #endregion

    }

    #endregion

}
