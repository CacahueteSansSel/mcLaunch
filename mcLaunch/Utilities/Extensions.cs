using System.Collections.Generic;
using System.Text;
using mcLaunch.Launchsite.Core;
using mcLaunch.Views.Popups;

namespace mcLaunch.Utilities;

public static class Extensions
{
    public static void ShowErrorPopup(this Result result)
    {
        if (!result.IsError) return;

        Navigation.ShowPopup(new MessageBoxPopup("Error occurred", result.ErrorMessage!, MessageStatus.Error));
    }

    public static string[] QuotesSplit(this string str, char separator)
    {
        bool inQuotes = false;
        List<string> tokens = [];
        StringBuilder current = new StringBuilder();
        
        foreach (var c in str)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == separator && !inQuotes)
            {
                tokens.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }
        
        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens.ToArray();
    }
}