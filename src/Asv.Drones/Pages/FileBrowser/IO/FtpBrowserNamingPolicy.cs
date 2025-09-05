using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Asv.Common;

namespace Asv.Drones;

public static partial class FtpBrowserNamingPolicy
{
    // MAVLINK_MSG_ID_FILE_TRANSFER_PROTOCOL spec. says that only 248 symbols allowed,
    // that is an approximate value, not including a path length
    public const int MaxNameLength = 100;
    public const int MinNameLength = 1;
    public static readonly string BlankName = Guid.NewGuid().ToString(); // TODO: prohibit empty names
    private static readonly Regex AllowedChars = AllowedCharsRegex();
    private const string AllowedCharsPattern = @"[A-Za-z0-9_.\-() ]";
    private const string AllowedNamePattern = "^" + AllowedCharsPattern + "+$";
    private static readonly Regex AllowedName = AllowedNameRegex();

    [GeneratedRegex(AllowedCharsPattern, RegexOptions.Compiled)]
    private static partial Regex AllowedCharsRegex();

    [GeneratedRegex(AllowedNamePattern, RegexOptions.Compiled)]
    private static partial Regex AllowedNameRegex();

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

        if (!AllowedName.IsMatch(name))
        {
            return ValidationResult.FailAsInvalidCharacters;
        }

        if (name.Length > MaxNameLength)
        {
            return ValidationResult.FailAsOutOfRange(
                MinNameLength.ToString(),
                MaxNameLength.ToString()
            );
        }

        return ValidationResult.Success;
    }
}
