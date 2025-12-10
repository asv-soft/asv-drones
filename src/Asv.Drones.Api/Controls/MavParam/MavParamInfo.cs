using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Asv.Avalonia;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Material.Icons;

namespace Asv.Drones.Api;

public enum MavParamWidgetType
{
    TextBox,
    ComboBox,
    Button,
    Latitude,
    Longitude,
    Altitude,
    AsciiChars,
}

public partial class MavParamInfo
{
    #region Parsing metadata long description

    private const string LongDescRegexString =
        @"^(?<description>[^\[\]]*?)(?:\s*\[(?<metadata>[^\]]*)\])?$";

    [GeneratedRegex(LongDescRegexString, RegexOptions.Compiled)]
    private static partial Regex CreateLongDescRegex();

    private static readonly Regex LongDescRegex = CreateLongDescRegex();

    private const string MetadataRegexString = @"(?<key>[^\;=]+)=(?<value>[^\;]+)(?:;|$)";

    [GeneratedRegex(MetadataRegexString, RegexOptions.Compiled)]
    private static partial Regex CreateMetadataRegex();

    private static readonly Regex MetadataRegex = CreateMetadataRegex();

    private static void ParseAdditionalInfo(
        string? metadataLongDesc,
        out ImmutableDictionary<string, string> additionalInfo,
        out string description
    )
    {
        description = string.Empty;
        additionalInfo = ImmutableDictionary<string, string>.Empty;

        if (string.IsNullOrWhiteSpace(metadataLongDesc))
        {
            return;
        }

        var match = LongDescRegex.Match(metadataLongDesc.Trim());
        if (!match.Success)
        {
            return;
        }

        description = match.Groups["description"].Value.Trim();
        var metadataString = match.Groups["metadata"].Value.Trim();
        if (string.IsNullOrWhiteSpace(metadataString))
        {
            return;
        }

        var metadataMatches = MetadataRegex.Matches(metadataString);
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        foreach (Match m in metadataMatches)
        {
            if (m.Groups["key"].Success && m.Groups["value"].Success)
            {
                builder[m.Groups["key"].Value.Trim()] = m.Groups["value"].Value.Trim();
            }
        }
        additionalInfo = builder.ToImmutable();
    }

    #endregion

    #region Well known keys

    public const string WidgetTypeKey = "widget";
    public const string FormatStringKey = "format";
    public const string IconKey = "icon";
    public const string IconColorKey = "icon-color";
    public const string OrderKey = "order";

    #endregion

    private readonly IMavParamTypeMetadata _metadata;
    private readonly ImmutableDictionary<string, string> _additionalInfo;
    private readonly string _description;

    public MavParamInfo(IMavParamTypeMetadata metadata)
    {
        _metadata = metadata;
        ParseAdditionalInfo(metadata.LongDesc, out _additionalInfo, out _description);
    }

    #region AdditionalInfo

    public ImmutableDictionary<string, string> AdditionalInfo => _additionalInfo;

    private static T? GetAdditionalAsEnum<T>(ImmutableDictionary<string, string> dict, string key)
        where T : struct
    {
        if (dict.TryGetValue(key, out var value))
        {
            value = NormalizeAdditionalValue(value);
            if (Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }
        }
        return null;
    }

    private static int GetAdditionalAsInt(
        ImmutableDictionary<string, string> dict,
        string key,
        int defaultValue = 0
    )
    {
        if (dict.TryGetValue(key, out var value))
        {
            value = NormalizeAdditionalValue(value);
            if (int.TryParse(value, out var result))
            {
                return result;
            }
        }
        return defaultValue;
    }

    private static double GetAdditionalAsDouble(
        ImmutableDictionary<string, string> dict,
        string key,
        double defaultValue = 0
    )
    {
        if (dict.TryGetValue(key, out var value))
        {
            value = NormalizeAdditionalValue(value).Replace(',', Units.DecimalSeparator);
            if (double.TryParse(value, NumberStyles.Any, null, out var result))
            {
                return result;
            }
        }
        return defaultValue;
    }

    private static string NormalizeAdditionalValue(string value)
    {
        return value.Trim().Replace("-", string.Empty);
    }

    #endregion

    public string Description => _description;

    public IMavParamTypeMetadata Metadata => _metadata;
    public string Id => _metadata.Name;
    public ValueType DefaultValue => Convert(Metadata.DefaultValue);
    public ValueType Max => Convert(Metadata.MaxValue);
    public ValueType Min => Convert(Metadata.MinValue);
    public ValueType Increment => Convert(Metadata.Increment);
    public string? FormatString =>
        CollectionExtensions.GetValueOrDefault(_additionalInfo, FormatStringKey);
    public MavParamWidgetType WidgetType =>
        GetAdditionalAsEnum<MavParamWidgetType>(_additionalInfo, WidgetTypeKey)
        ?? MavParamWidgetType.TextBox;
    public MaterialIconKind? Icon =>
        GetAdditionalAsEnum<MaterialIconKind>(_additionalInfo, IconKey);

    public AsvColorKind? IconColor =>
        GetAdditionalAsEnum<AsvColorKind>(_additionalInfo, IconColorKey);

    public int Order => GetAdditionalAsInt(_additionalInfo, OrderKey);
    public string Title => Metadata.ShortDesc ?? Metadata.Name;

    public IEnumerable<MavParamValueItem> GetPredefinedValues()
    {
        if (Metadata.Values == null)
        {
            yield break;
        }

        foreach (var value in Metadata.Values)
        {
            ParseAdditionalInfo(value.Item2, out var dict, out var desc);
            var icon = GetAdditionalAsEnum<MaterialIconKind>(dict, IconKey);
            var iconColor = GetAdditionalAsEnum<AsvColorKind>(_additionalInfo, IconKey);

            yield return new MavParamValueItem(
                icon,
                iconColor,
                desc,
                value.Item1,
                Convert(value.Item1)
            );
        }
    }

    public MavParamValue Convert(ValueType value)
    {
        return Metadata.Type switch
        {
            MavParamType.MavParamTypeUint8 => new MavParamValue(System.Convert.ToByte(value)),
            MavParamType.MavParamTypeInt8 => new MavParamValue(System.Convert.ToSByte(value)),
            MavParamType.MavParamTypeUint16 => new MavParamValue(System.Convert.ToUInt16(value)),
            MavParamType.MavParamTypeInt16 => new MavParamValue(System.Convert.ToInt16(value)),
            MavParamType.MavParamTypeUint32 => new MavParamValue(System.Convert.ToUInt32(value)),
            MavParamType.MavParamTypeInt32 => new MavParamValue(System.Convert.ToInt32(value)),
            MavParamType.MavParamTypeReal32 => new MavParamValue(System.Convert.ToSingle(value)),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public ValueType Convert(MavParamValue value)
    {
        Debug.Assert(
            value.Type == Metadata.Type,
            $"Value type {value.Type} does not match metadata type {Metadata.Type} for param {Metadata.Name}"
        );
        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                return (byte)value;
            case MavParamType.MavParamTypeInt8:
                return (sbyte)value;
            case MavParamType.MavParamTypeUint16:
                return (ushort)value;
            case MavParamType.MavParamTypeInt16:
                return (short)value;
            case MavParamType.MavParamTypeUint32:
                return (uint)value;
            case MavParamType.MavParamTypeInt32:
                return (int)value;
            case MavParamType.MavParamTypeReal32:
                return (float)value;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public string? Print(ValueType? value)
    {
        if (value == null)
        {
            return null;
        }

        if (FormatString == null)
        {
            return value.ToString();
        }

        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                return ((byte)value).ToString(FormatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeInt8:
                return ((sbyte)value).ToString(FormatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeUint16:
                return ((ushort)value).ToString(FormatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeInt16:
                return ((short)value).ToString(FormatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeUint32:
                return ((uint)value).ToString(FormatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeInt32:
                return ((int)value).ToString(FormatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeReal32:
                return ((float)value).ToString(FormatString, CultureInfo.InvariantCulture);
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public string? GetError(ValueType? value)
    {
        if (value == null)
        {
            return "Value is required";
        }
        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                var byteVal = System.Convert.ToByte(value);
                if (byteVal > System.Convert.ToByte(Max) || byteVal < System.Convert.ToByte(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeInt8:
                var sbyteVal = System.Convert.ToSByte(value);
                if (
                    sbyteVal > System.Convert.ToSByte(Max)
                    || sbyteVal < System.Convert.ToSByte(Min)
                )
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeUint16:
                var ushortVal = System.Convert.ToUInt16(value);
                if (
                    ushortVal > System.Convert.ToUInt16(Max)
                    || ushortVal < System.Convert.ToUInt16(Min)
                )
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeInt16:
                var shortVal = System.Convert.ToInt16(value);
                if (
                    shortVal > System.Convert.ToInt16(Max)
                    || shortVal < System.Convert.ToInt16(Min)
                )
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeUint32:
                var uintVal = System.Convert.ToUInt32(value);
                if (
                    uintVal > System.Convert.ToUInt32(Max)
                    || uintVal < System.Convert.ToUInt32(Min)
                )
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeInt32:
                var intVal = System.Convert.ToInt32(value);
                if (intVal > System.Convert.ToInt32(Max) || intVal < System.Convert.ToInt32(Min))
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeReal32:
                var floatVal = System.Convert.ToSingle(value);
                if (
                    floatVal > System.Convert.ToSingle(Max)
                    || floatVal < System.Convert.ToSingle(Min)
                )
                {
                    return $"Value must be in range {Min}..{Max}";
                }
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public bool IsValid(ValueType? value)
    {
        if (value == null)
        {
            return false;
        }
        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                var byteVal = System.Convert.ToByte(value);
                if (byteVal > System.Convert.ToByte(Max) || byteVal < System.Convert.ToByte(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeInt8:
                var sbyteVal = System.Convert.ToSByte(value);
                if (
                    sbyteVal > System.Convert.ToSByte(Max)
                    || sbyteVal < System.Convert.ToSByte(Min)
                )
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeUint16:
                var ushortVal = System.Convert.ToUInt16(value);
                if (
                    ushortVal > System.Convert.ToUInt16(Max)
                    || ushortVal < System.Convert.ToUInt16(Min)
                )
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeInt16:
                var shortVal = System.Convert.ToInt16(value);
                if (
                    shortVal > System.Convert.ToInt16(Max)
                    || shortVal < System.Convert.ToInt16(Min)
                )
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeUint32:
                var uintVal = System.Convert.ToUInt32(value);
                if (
                    uintVal > System.Convert.ToUInt32(Max)
                    || uintVal < System.Convert.ToUInt32(Min)
                )
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeInt32:
                var intVal = System.Convert.ToInt32(value);
                if (intVal > System.Convert.ToInt32(Max) || intVal < System.Convert.ToInt32(Min))
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeReal32:
                var floatVal = System.Convert.ToSingle(value);
                if (
                    floatVal > System.Convert.ToSingle(Max)
                    || floatVal < System.Convert.ToSingle(Min)
                )
                {
                    return false;
                }
                break;
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            case MavParamType.MavParamTypeReal64:
            default:
                throw new ArgumentOutOfRangeException();
        }
        return true;
    }

    public Exception? ValidateString(string valueAsString, out ValueType value)
    {
        value = DefaultValue;
        if (string.IsNullOrWhiteSpace(valueAsString))
        {
            return new Exception("Value is empty");
        }
        valueAsString = valueAsString
            .Replace(',', Units.DecimalSeparator)
            .Trim(' ')
            .Replace(" ", string.Empty);

        if (valueAsString.Length == 0)
        {
            return new Exception("Value is empty");
        }
        var lastChar = valueAsString[^1];
        int multiply;
        switch (lastChar)
        {
            case 'M' or 'm' or 'М' or 'м':
                multiply = 1_000_000;
                valueAsString = valueAsString[..^1];
                break;
            case 'K' or 'k' or 'К' or 'к':
                multiply = 1_000;
                valueAsString = valueAsString[..^1];
                break;
            case 'G' or 'g' or 'Г' or 'г':
                multiply = 1_000_000_000;
                valueAsString = valueAsString[..^1];
                break;
            default:
                multiply = 1;
                break;
        }

        if (
            double.TryParse(
                valueAsString,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var doubleValue
            )
        )
        {
            doubleValue *= multiply;
        }
        else
        {
            return new Exception("Value must be a number with optional suffix (K, M, G)");
        }

        switch (Metadata.Type)
        {
            case MavParamType.MavParamTypeUint8:
                value = (byte)doubleValue;
                break;
            case MavParamType.MavParamTypeInt8:
                value = (sbyte)doubleValue;
                break;
            case MavParamType.MavParamTypeUint16:
                value = (ushort)doubleValue;
                break;

            case MavParamType.MavParamTypeInt16:
                value = (short)doubleValue;
                break;

            case MavParamType.MavParamTypeUint32:
                value = (uint)doubleValue;
                break;
            case MavParamType.MavParamTypeInt32:
                value = (int)doubleValue;
                break;
            case MavParamType.MavParamTypeReal32:
                value = (float)doubleValue;
                break;
            case MavParamType.MavParamTypeReal64:
            case MavParamType.MavParamTypeUint64:
            case MavParamType.MavParamTypeInt64:
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (IsValid(value) == false)
        {
            return new Exception(GetError(value));
        }

        return null;
    }
}

public class MavParamValueItem(
    MaterialIconKind? icon,
    AsvColorKind? iconColor,
    string title,
    MavParamValue mavlinkValue,
    ValueType value
)
{
    public MaterialIconKind? Icon => icon;
    public AsvColorKind IconColor => iconColor ?? AsvColorKind.None;
    public string Title => title;
    public MavParamValue MavValue => mavlinkValue;
    public ValueType Value => value;
}
