using System.Text;

namespace Kernel.Extensions;

public static class StringExtensions
{
    public static string ToKebabCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder();
        var previousWasHyphen = false;

        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];

            // If uppercase letter -> maybe insert hyphen (if not first and previous char not hyphen)
            if (char.IsUpper(c))
            {
                if (i > 0 && !previousWasHyphen && sb.Length > 0)
                    sb.Append('-');

                sb.Append(char.ToLowerInvariant(c));
                previousWasHyphen = false;
                continue;
            }

            // treat spaces, underscores, dots as separators -> hyphen
            if (char.IsWhiteSpace(c) || c == '_' || c == '.')
            {
                if (!previousWasHyphen && sb.Length > 0)
                {
                    sb.Append('-');
                    previousWasHyphen = true;
                }
                continue;
            }

            // keep letters and digits and dash
            if (char.IsLetterOrDigit(c) || c == '-')
            {
                sb.Append(char.ToLowerInvariant(c));
                previousWasHyphen = (c == '-');
                continue;
            }

            // skip other punctuation/symbols (or you can choose to keep them)
            // e.g. to keep them: sb.Append(c);
        }

        // Trim leading/trailing hyphens and collapse duplicates already handled
        var result = sb.ToString().Trim('-');

        return result;
    }
}
