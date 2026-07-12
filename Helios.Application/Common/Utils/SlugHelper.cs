using System.Text;
using System.Text.RegularExpressions;

namespace Helios.Application.Common.Utils;

public static class SlugHelper
{
    public static string GenerateSlug(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase))
            return string.Empty;

        string str = RemoveDiacritics(phrase).ToLowerInvariant();

        // invalid chars           
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        // convert multiple spaces into one space   
        str = Regex.Replace(str, @"\s+", " ").Trim();
        // cut and trim 
        str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
        str = Regex.Replace(str, @"\s", "-"); // hyphens   

        return str;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                if (c == 'ı') stringBuilder.Append('i');
                else if (c == 'ğ') stringBuilder.Append('g');
                else if (c == 'ü') stringBuilder.Append('u');
                else if (c == 'ş') stringBuilder.Append('s');
                else if (c == 'ö') stringBuilder.Append('o');
                else if (c == 'ç') stringBuilder.Append('c');
                else if (c == 'İ') stringBuilder.Append('i');
                else if (c == 'Ğ') stringBuilder.Append('g');
                else if (c == 'Ü') stringBuilder.Append('u');
                else if (c == 'Ş') stringBuilder.Append('s');
                else if (c == 'Ö') stringBuilder.Append('o');
                else if (c == 'Ç') stringBuilder.Append('c');
                else stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
