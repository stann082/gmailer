namespace core;

public static class Extensions
{

    #region Public Methods

    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than zero.");

        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return YieldBatchElements(enumerator, batchSize - 1);
        }
    }

    public static void DetermineDomains(this IEnumerable<Email>? emails)
    {
        if (emails == null) return;
        foreach (var email in emails)
        {
            if (string.IsNullOrEmpty(email.Sender))
            {
                continue;
            }

            if (!email.Sender.Contains('@'))
            {
                email.Domain = email.Sender;
                continue;
            }

            string[] recipientSplit = email.Sender.Split('@');
            string[]? domainParts = recipientSplit.LastOrDefault()?.Split('.');
            string? lastTwoParts = domainParts?.Length >= 2 ? string.Join('.', domainParts, domainParts.Length - 2, 2) : recipientSplit.LastOrDefault();
            email.Domain = lastTwoParts?.Trim('<', '>');
        }
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
    {
        yield return source.Current;
        for (int i = 0; i < batchSize && source.MoveNext(); i++)
        {
            yield return source.Current;
        }
    }
    
    #endregion
    
}
