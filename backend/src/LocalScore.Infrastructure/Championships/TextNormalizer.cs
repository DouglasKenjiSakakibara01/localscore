using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LocalScore.Infrastructure.Championships;

internal static partial class TextNormalizer
{
    public static string NormalizeTeamName(string value) =>
        value.Trim().ToUpperInvariant();

    public static string? Optional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public static string? UpperOptional(string? value) =>
        Optional(value)?.ToUpperInvariant();

    public static string Slugify(string value)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsAsciiLetterOrDigit(character)
                ? char.ToLowerInvariant(character)
                : '-');
        }

        return RepeatedHyphens().Replace(builder.ToString(), "-").Trim('-');
    }

    public static string SortKey(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("-+")]
    private static partial Regex RepeatedHyphens();
}
