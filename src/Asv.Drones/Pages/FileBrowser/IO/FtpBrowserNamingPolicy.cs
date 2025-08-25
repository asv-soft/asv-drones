using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Asv.Common;

namespace Asv.Drones;

public static partial class FtpBrowserNamingPolicy
{
    public const int MaxNameLength = 248; // see MAVLINK_MSG_ID_FILE_TRANSFER_PROTOCOL spec.
    public static readonly string BlankName = Guid.NewGuid().ToString(); // TODO: prohibit empty names
    private static readonly Regex AllowedChars = AllowedCharsRegex();
    public const string AllowedCharsPattern = @"[A-Za-z0-9_.\-() ]";

    [GeneratedRegex(AllowedCharsPattern, RegexOptions.Compiled)]
    private static partial Regex AllowedCharsRegex();

    public static string SanitizeForDisplay(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(AllowedChars.IsMatch(ch.ToString()) ? ch : '*');
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
}
