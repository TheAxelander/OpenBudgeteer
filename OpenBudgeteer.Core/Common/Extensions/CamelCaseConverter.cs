using System.Text;

namespace OpenBudgeteer.Core.Common.Extensions;

public static class CamelCaseConverter
{
    public static string ConvertToSpaces(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase)) return camelCase;

        var result = new StringBuilder(camelCase.Length * 2);
        result.Append(camelCase[0]);

        for (var i = 1; i < camelCase.Length; i++)
        {
            if (char.IsUpper(camelCase[i])) result.Append(' ');
            result.Append(camelCase[i]);
        }

        return result.ToString();
    }
}