using System.Collections.ObjectModel;
using core;

namespace ui.Views;

public partial class GroupListView
{

    #region Constructors

    public GroupListView()
    {
        InitializeComponent();
    }

    #endregion

    #region Event Definitions

    public event EventHandler<EmailGrouping>? GroupSelected;

    #endregion

    #region Public Methods

    public void SetItemsSource(IEnumerable<EmailGrouping> groups) => GroupList.ItemsSource = groups;

    public EmailGrouping? CurrentSelection => GroupList.SelectedItem as EmailGrouping;

    public ObservableCollection<EmailGrouping>? ItemsSource => GroupList.ItemsSource as ObservableCollection<EmailGrouping>;
    
    #endregion

    #region Helper Methods

    private void OnGroupSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is EmailGrouping g)
        {
            GroupSelected?.Invoke(this, g);
        }
    }

    #endregion
    
}
