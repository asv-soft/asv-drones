using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum LongitudeUnits
{
    Degrees,
    DegreesMinutesSeconds
}
public class Longitude : MeasureUnitBase<double,LongitudeUnits>
{
    private static readonly IMeasureUnitItem<double, LongitudeUnits>[] Units = {
        new LongitudeUnitDegrees(),
        new LongitudeUnitDegreesMinutesSeconds()
    };
    public Longitude(IConfiguration cfgSvc,string cfgKey):base(cfgSvc,cfgKey,Units)
    {
        
    }

    public override string Title => "Longitude";
    public override string Description => "Units of longitude";
}

public class LongitudeUnitDegrees : DoubleMeasureUnitItem<LongitudeUnits>
{
    public LongitudeUnitDegrees() : base(LongitudeUnits.Degrees, "[Deg]°", "°", true, "F7", 1)
    {

    }

    public override bool IsValid(string value)
    {
        if (base.IsValid(value) == false) return false;
        var val = ConvertToSi(value);
        return val is >= -180 and <= 180;
    }

    public override string? GetErrorMessage(string value)
    {
        var msg = base.GetErrorMessage(value);
        if (msg != null) return msg;
        var val = ConvertToSi(value);
        if (val < -180) return "Longitude must be greater than -180°"; // TODO: Localize
        if (val > 180) return "Longitude must be less than 180°"; // TODO: Localize
        return null;

    }
}

public class LongitudeUnitDegreesMinutesSeconds : DoubleMeasureUnitItem<LongitudeUnits>
{
    public LongitudeUnitDegreesMinutesSeconds() : base(LongitudeUnits.DegreesMinutesSeconds,
        "[Deg]°[Min]′[Sec]′′", "°", true, "F7", 1)
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
        if (val < -180) return "Longitude must be greater than -180°"; // TODO: Localize
        if (val > 180) return "Longitude must be less than 180°"; // TODO: Localize
        return null;

    }

    public override string FromSiToString(double value)
    {
        var minutes = (value - (int)value) * 60;
        return $"{value:F0}°{minutes:F0}′{(minutes - (int)minutes) * 60:F2}′′";
    }

    public override string FromSiToStringWithUnits(double value)
    {
        return FromSiToString(value);
    }
}