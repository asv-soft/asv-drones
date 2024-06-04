using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Material.Icons;

namespace Asv.Drones.Gui.Api;

[PseudoClasses(":critical", ":warning", ":normal", ":unknown")]
public class GpsStatusIndicator : IndicatorBase
{
    #region Styled Props

    public static readonly StyledProperty<GpsFixType> FixTypeProperty =
        AvaloniaProperty.Register<GpsStatusIndicator, GpsFixType>(
            nameof(FixType), GpsFixType.GpsFixTypeNoGps, coerce: UpdateValue);

    public GpsFixType FixType
    {
        get => GetValue(FixTypeProperty);
        set => SetValue(FixTypeProperty, value);
    }

    public static readonly StyledProperty<string> ToolTipTextProperty =
        AvaloniaProperty.Register<GpsStatusIndicator, string>(
            nameof(ToolTipText), string.Empty);

    public string ToolTipText
    {
        get => GetValue(ToolTipTextProperty);
        set => SetValue(ToolTipTextProperty, value);
    }

    public static readonly StyledProperty<DopStatusEnum> DopStatusProperty =
        AvaloniaProperty.Register<GpsStatusIndicator, DopStatusEnum>(
            nameof(DopStatus), DopStatusEnum.Unknown, coerce: UpdateValue);

    private static DopStatusEnum UpdateValue(AvaloniaObject arg1, DopStatusEnum arg2)
    {
        // TODO: AVALONIA11 => implement this
        return DopStatusEnum.Excellent;
    }

    public DopStatusEnum DopStatus
    {
        get => GetValue(DopStatusProperty);
        set => SetValue(DopStatusProperty, value);
    }

    #endregion

    public static void SetPseudoClass(GpsStatusIndicator indicator)
    {
        var dopStatus = indicator.DopStatus;

        indicator.PseudoClasses.Set(":unknown", dopStatus == DopStatusEnum.Unknown);
        indicator.PseudoClasses.Set(":critical", dopStatus is DopStatusEnum.Fair or DopStatusEnum.Poor);
        indicator.PseudoClasses.Set(":warning", dopStatus == DopStatusEnum.Moderate);
        indicator.PseudoClasses.Set(":normal",
            dopStatus is DopStatusEnum.Ideal or DopStatusEnum.Excellent or DopStatusEnum.Good);

        indicator.IconKind = GetIcon(indicator.FixType);
        indicator.ToolTipText = indicator.FixType.GetShortDisplayName();
    }

    private static GpsFixType UpdateValue(AvaloniaObject avaloniaObject, GpsFixType gpsFixType)
    {
        // TODO: AVALONIA11 => implement this
        if (avaloniaObject is not GpsStatusIndicator indicator) return GpsFixType.GpsFixType2dFix;
        SetPseudoClass(indicator);
        return GpsFixType.GpsFixType2dFix;
    }

    private static MaterialIconKind GetIcon(GpsFixType fixType)
    {
        return fixType switch
        {
            GpsFixType.GpsFixTypeNoGps => MaterialIconKind.CrosshairsQuestion,
            GpsFixType.GpsFixTypeNoFix => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixType2dFix => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixType3dFix => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixTypeDgps => MaterialIconKind.Crosshairs,
            GpsFixType.GpsFixTypeRtkFloat => MaterialIconKind.CrosshairsGps,
            GpsFixType.GpsFixTypeRtkFixed => MaterialIconKind.CrosshairsGps,
            GpsFixType.GpsFixTypeStatic => MaterialIconKind.CrosshairsGps,
            GpsFixType.GpsFixTypePpp => MaterialIconKind.CrosshairsGps,
            _ => throw new ArgumentOutOfRangeException(nameof(fixType), fixType, null)
        };
    }
}