using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum LatitudeUnits
{
    Degrees,
    DegreesMinutesSeconds
}

public class Latitude : MeasureUnitBase<double,LatitudeUnits>
{
    private static readonly IMeasureUnitItem<double, LatitudeUnits>[] Units = {
        new LatitudeUnitDegrees(),
        new LatitudeUnitDegreesMinutesSeconds()
    };
    public Latitude(IConfiguration cfgSvc,string cfgKey):base(cfgSvc,cfgKey,Units)
    {
        
    }

    public override string Title => "Latitude";
    public override string Description => "Units of latitude";
}

public class LatitudeUnitDegrees : DoubleMeasureUnitItem<LatitudeUnits>
{
    public LatitudeUnitDegrees() : base(LatitudeUnits.Degrees, "Degrees", "°", true, "F7", 1)
    {

    }

    public override bool IsValid(string value)
    {
        if (base.IsValid(value) == false) return false;
        var val = ConvertToSi(value);
        return val is >= -90 and <= 90;
    }

    public override string? GetErrorMessage(string value)
    {
        var msg = base.GetErrorMessage(value);
        if (msg != null) return msg;
        var val = ConvertToSi(value);
        if (val < -90) return "Latitude must be greater than -90°"; // TODO: Localize
        if (val > 90) return "Latitude must be less than 90°"; // TODO: Localize
        return null;

    }
}

public class LatitudeUnitDegreesMinutesSeconds : DoubleMeasureUnitItem<LatitudeUnits>
{
    public LatitudeUnitDegreesMinutesSeconds() : base(LatitudeUnits.DegreesMinutesSeconds,
        "Degrees, minutes, seconds", "°", true, "F7", 1)
    {

    }

    public override bool IsValid(string value)
    {
        //TODO: Parse strings like "N 40° 26.771′"
        if (base.IsValid(value) == false) return false;
        var val = ConvertToSi(value);
        return val is >= -90 and <= 90;
    }

    public override string? GetErrorMessage(string value)
    {
        //TODO: Parse strings like "N 40° 26.771′"
        var msg = base.GetErrorMessage(value);
        if (msg != null) return msg;
        var val = ConvertToSi(value);
        if (val < -90) return "Latitude must be greater than -90°"; // TODO: Localize
        if (val > 90) return "Latitude must be less than 90°"; // TODO: Localize
        return null;

    }

    public override string FromSiToString(double value)
    {
        var minutes = (value - (int)value) * 60;
        return $"{value:F0}°{minutes:F0}'{(minutes - (int)minutes) * 60:F2}''";
    }

    public override string FromSiToStringWithUnits(double value)
    {
        return FromSiToString(value);
    }
}