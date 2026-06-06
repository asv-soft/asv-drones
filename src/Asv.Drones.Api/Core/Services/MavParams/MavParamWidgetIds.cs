namespace Asv.Drones.Api;

public static class MavParamWidgetIds
{
    public const string TextBox = "textbox";
    public const string AsciiChars = "ascii-chars";
    public const string ComboBox = "combo-box";
    public const string Button = "button";
    public const string Altitude = "altitude";
    public const string Latitude = "latitude";
    public const string Longitude = "longitude";
    public const string Hidden = "hidden";

    public static string GetIdByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TextBox;
        }

        var trimmed = name.Trim();
        var compactName = trimmed
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);

        return compactName.ToLowerInvariant() switch
        {
            "textbox" => TextBox,
            "asciichars" => AsciiChars,
            "combobox" => ComboBox,
            "button" => Button,
            "altitude" => Altitude,
            "latitude" => Latitude,
            "longitude" => Longitude,
            "hidden" => Hidden,
            _ => NormalizeCustomId(trimmed),
        };
    }

    private static string NormalizeCustomId(string name)
    {
        return name.Trim().Replace("_", "-").Replace(" ", "-").ToLowerInvariant();
    }
}
