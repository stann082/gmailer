using core;
using core.nullobj;
using service;
using Syncfusion.Maui.ListView;
using ui.model;

namespace ui;

public partial class MainPage
{

    #region Constructors

    public MainPage(IEmailService emailService)
    {
        InitializeComponent();
        _selectionCache = new List<EmailGrouping>();
        BindingContext = new MainPageViewModel(emailService, _selectionCache);
        btnDelete.IsEnabled = false;
        busyIndicator.IsRunning = true;

        MessagesOptions options = new MessagesOptions();
        options.ShouldGetCache = true;
        options.Label = "inbox";
        options.ResultsPePage = 100;

        EmailRepository viewModel = InitializeEmailRepository(emailService, options);
        InitializeListView(viewModel);
        
        busyIndicator.IsRunning = false;
    }

    #endregion

    #region Variables

    private readonly IList<EmailGrouping> _selectionCache;

    #endregion

    #region Event Handlers

    private void Popup_Clicked(object sender, EventArgs e)
    {
        if (listView.SelectedItems == null)
        {
            return;
        }

        foreach (var selectedItem in listView.SelectedItems)
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
        if (listView.SelectedItems != null) btnDelete.IsEnabled = listView.SelectedItems.Any();
    }

    #endregion

    #region Helper Methods

    private static EmailRepository InitializeEmailRepository(IEmailService emailService, MessagesOptions options)
    {
        EmailGroupingCollection grouping = emailService.ListEmails(options);
        EmailRepository viewModel = new EmailRepository();
        foreach (var email in grouping.GetGroupings().OrderByDescending(g => g.Total))
        {
            viewModel.Emails.Add(email);
        }

        return viewModel;
    }

    private void InitializeListView(EmailRepository viewModel)
    {
        listView.ItemsSource = viewModel.Emails;
        listView.ItemTemplate = new DataTemplate(() =>
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
