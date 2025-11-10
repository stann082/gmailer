namespace core;

public class EmailGrouping(IGrouping<string?, Email> group)
{

    #region Properties

    public string? Domain { get; } = group.Key;
    public Email[] Emails { get; } = group.ToArray();
    public string Id { get; } = Guid.NewGuid().ToString();
    public int Total { get; } = group.Count();

    #endregion

}
