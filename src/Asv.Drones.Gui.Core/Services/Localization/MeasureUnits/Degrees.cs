﻿using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core;

public enum DegreeUnits
{
    Degrees,
    DegreesMinutesSeconds
}

public class Degrees : MeasureUnitBase<double, DegreeUnits>
{
    private static readonly IMeasureUnitItem<double, DegreeUnits>[] _units = {
        new DegreesMeasureUnit(),
        new DmsMeasureUnit()
    };
    
    public Degrees(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Degrees_Title;
    public override string Description => RS.Degrees_Description;
}

public class DegreesMeasureUnit : IMeasureUnitItem<double, DegreeUnits>
{
    public DegreeUnits Id => DegreeUnits.Degrees;
    public string Title => RS.Degrees_Degree_Title;
    public string Unit => RS.Degrees_Degree_Title;
    public bool IsSiUnit => true;
    //Not usable
    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }
    //Not usable
    public double ConvertToSi(double value)
    {
        return value;
    }

    public bool IsValid(string value)
    {
        return Angle.IsValid(value);
    }

    public string? GetErrorMessage(string value)
    {
        return Angle.GetErrorMessage(value);
    }

    public double ConvertToSi(string value)
    {
        return Angle.TryParse(value, out var result) ? result : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return $"{value}";
    }

    public string FromSiToStringWithUnits(double value)
    {
        return $"{value} {Unit}";
    }
}

public class DmsMeasureUnit : IMeasureUnitItem<double, DegreeUnits>
{
    public DegreeUnits Id => DegreeUnits.DegreesMinutesSeconds;
    public string Title => RS.Degrees_DMS_Title;
    public string Unit => RS.Degrees_DMS_Title;
    public bool IsSiUnit => false;
    //Not usable
    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }
    //Not usable
    public double ConvertToSi(double value)
    {
        return value;
    }

    public bool IsValid(string value)
    {
        return Angle.IsValid(value);
    }

    public string? GetErrorMessage(string value)
    {
        return Angle.GetErrorMessage(value);
    }

    public double ConvertToSi(string value)
    {
        return Angle.TryParse(value, out var result) ? result : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return Angle.PrintDms(value);
    }

    public string FromSiToStringWithUnits(double value)
    {
        return Angle.PrintDms(value);
    }
}