﻿namespace core;

public static class Extensions
{

    public static void DetermineDomains(this IEnumerable<Email> emails)
    {
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
            email.Domain = domainParts?.Length >= 2 ? string.Join('.', domainParts, domainParts.Length - 2, 2) : recipientSplit.LastOrDefault();
        }
    }

}
