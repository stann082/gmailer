using core.nullobj;
using service;
using ui.model;
using Email = core.Email;

namespace ui;

public partial class MainPage
{

    #region Constructors

    public MainPage(IEmailService emailService)
    {
        InitializeComponent();

        MessagesOptions options = new MessagesOptions();
        options.ShouldGetCache = true;
        options.Label = "inbox";
        options.MaxResults = 50;

        Email[] emails = emailService.ListEmails(options);
        EmailModel context = new EmailModel();
        foreach (var email in emails)
        {
            context.Emails.Add(email);
        }

        dataGrid.ItemsSource = context.Emails;
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
