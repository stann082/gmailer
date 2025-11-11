using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace core;

public class EmailGrouping : INotifyPropertyChanged
{
    
    public EmailGrouping(IGrouping<string?, Email> group)
    {
        Domain = group.Key;
        Emails = new ObservableCollection<Email>(group.ToList());
        Emails.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Total));
    }

    #region Properties

    public string? Domain { get; }
    public ObservableCollection<Email> Emails { get; }
    public string Id { get; } = Guid.NewGuid().ToString();
    public int Total => Emails.Count;

    #endregion

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}
