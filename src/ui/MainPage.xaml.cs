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
        BindingContext = new MainPageViewModel(emailService);
        btnDelete.IsEnabled = false;

        MessagesOptions options = new MessagesOptions();
        options.ShouldGetCache = true;
        options.Label = "inbox";
        options.MaxResults = 50;

        EmailGroupingCollection grouping = emailService.ListEmails(options);
        EmailRepository viewModel = new EmailRepository();
        foreach (var email in grouping.GetGroupings().OrderByDescending(g => g.Total))
        {
            viewModel.Emails.Add(email);
        }

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

    #region Event Handlers

    private void Popup_Clicked(object sender, EventArgs e)
    {
        if (listView.SelectedItem is not EmailGrouping)
        {
            return;
        }

        popup.Show();
    }

    private void ListView_OnSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
    {
        if (listView.SelectedItems != null) btnDelete.IsEnabled = listView.SelectedItems.Any();
    }

    #endregion

    #region Helper Classes

    // TODO: bind IMessagesOptions to the UI controls
    private class MessagesOptions : NullMessagesOptions
    {

        #region Properties

        public override string Label { get; set; }
        public override int MaxResults { get; set; }
        public override bool ShouldGetCache { get; set; }

        #endregion

    }

    #endregion

}
