using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Asv.Common;

namespace Asv.Drones;

public static partial class BrowserNamingPolicy
{
    public const string AllowedCharsPattern = @"[A-Za-z0-9_.\-() ]";
    public const int MaxNameLength = 255;
    public const string BlankName = "UNNAMED";

    private static readonly Regex AllowedChar = AllowedCharsRegex();

    public static string SanitizeForDisplay(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(AllowedChar.IsMatch(ch.ToString()) ? ch : '*');
        }

        return sb.ToString();
    }

    public static ValidationResult Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ValidationResult.FailAsNullOrWhiteSpace;
        }

        if (name.Any(Path.GetInvalidFileNameChars().Contains))
        {
            return ValidationResult.FailAsInvalidCharacters;
        }

        if (name.Length > MaxNameLength)
        {
            return ValidationResult.FailAsOutOfRange("1", MaxNameLength.ToString());
        }

        return ValidationResult.Success;
    }

    [GeneratedRegex(AllowedCharsPattern, RegexOptions.Compiled)]
    private static partial Regex AllowedCharsRegex();
}
