using System.Globalization;
using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core;

public enum DdmUnits
{
    InParts,
    Percent,
    MicroAmp,
    MicroAmpRu
}

public class DdmLlz : MeasureUnitBase<double,DdmUnits>
{
    private const double AbsoluteInPercent = 0.01;
    private const double AbsoluteInMicroAmp = 0.155 / 150;
    private const double AbsoluteInMicroAmpRu = 0.155 / 250;
    
    private static readonly IMeasureUnitItem<double, DdmUnits>[] _units = {
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.InParts,RS.Ddm_InParts_Title,"1",true, "F4",1),
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.Percent,RS.Ddm_Percent_Title,"%",false,"F2",AbsoluteInPercent),
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.MicroAmp,RS.Ddm_Microamp_Title,RS.Ddm_µA_Unit,false,"F1",AbsoluteInMicroAmp),
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.MicroAmpRu,RS.Ddm_MicroampRus_Title,RS.Ddm_µA_Unit,false,"F1",AbsoluteInMicroAmpRu)
    };
    
    public DdmLlz(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.DdmLlz_Title;

    public override string Description => RS.DdmLlz_Description;
}

public class DdmLlzInParts : IMeasureUnitItem<double, DdmUnits>
{
    public DdmUnits Id => DdmUnits.InParts;
    public string Title => "[DDM]";
    public string Unit => "1";
    public bool IsSiUnit => true;
    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }

    public double ConvertToSi(double value)
    {
        return value;
    }

    public bool IsValid(string value)
    {
        if (value.IsNullOrWhiteSpace()) return false;
        if (double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
        {
            return v is >= -0.5 and <= 0.5;
        }
        return false;
    }

    public string GetErrorMessage(string value)
    {
        if (value.IsNullOrWhiteSpace()) return "Value can't be null or white space"; // TODO: Localize
        value = value.Replace(',', '.');
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) == false)
            return "Value must be a number";
        return v is >= -0.5 and <= 0.5 ? null : "The value must be greater than -0.5 and less than 0.5";
    }

    public double ConvertToSi(string value)
    {
        if (value.IsNullOrWhiteSpace()) return double.NaN;
        return double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return value.ToString("F4");
    }

    public string FromSiToStringWithUnits(double value)
    {
        return $"{value:F4} {Unit}";
    }
}

public class DdmLlzPercent : IMeasureUnitItem<double, DdmUnits>
{
    private const double SiInPercent = 100.0;
    public DdmUnits Id => DdmUnits.InParts;
    public string Title => "[DDM]";
    public string Unit => "%";
    public bool IsSiUnit => false;
    public double ConvertFromSi(double siValue)
    {
        return siValue * SiInPercent;
    }

    public double ConvertToSi(double value)
    {
        return value / SiInPercent;
    }

    public bool IsValid(string value)
    {
        if (value.IsNullOrWhiteSpace()) return false;
        if (double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
        {
            return v is >= -50.0 and <= 50.0;
        }
        return false;
    }

    public string GetErrorMessage(string value)
    {
        if (value.IsNullOrWhiteSpace()) return "Value can't be null or white space"; // TODO: Localize
        value = value.Replace(',', '.');
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) == false)
            return "Value must be a number";
        return v is >= -50.0 and <= 50.0 ? null : "The value must be greater than -50.0 and less than 50.0";
    }

    public double ConvertToSi(string value)
    {
        if (value.IsNullOrWhiteSpace()) return double.NaN;
        return double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v * SiInPercent : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return (value * SiInPercent).ToString("F2");
    }

    public string FromSiToStringWithUnits(double value)
    {
        return $"{value * SiInPercent:F2} {Unit}";
    }
}

public class DdmLlzMicroAmp : IMeasureUnitItem<double, DdmUnits>
{
    private const double SiInMicroAmp = 150 / 0.155;
    public DdmUnits Id => DdmUnits.InParts;
    public string Title => "[DDM]";
    public string Unit => "µA";
    public bool IsSiUnit => false;
    public double ConvertFromSi(double siValue)
    {
        return siValue * SiInMicroAmp;
    }

    public double ConvertToSi(double value)
    {
        return value / SiInMicroAmp;
    }

    public bool IsValid(string value)
    {
        if (value.IsNullOrWhiteSpace()) return false;
        if (double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
        {
            return v is >= -483.87 and <= 483.87;
        }
        return false;
    }

    public string GetErrorMessage(string value)
    {
        if (value.IsNullOrWhiteSpace()) return "Value can't be null or white space"; // TODO: Localize
        value = value.Replace(',', '.');
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) == false)
            return "Value must be a number";
        return v is >= -483.87 and <= 483.87 ? null : "The value must be greater than -483.87 and less than 483.87";
    }

    public double ConvertToSi(string value)
    {
        if (value.IsNullOrWhiteSpace()) return double.NaN;
        return double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v * SiInMicroAmp : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return (value * SiInMicroAmp).ToString("F1");
    }

    public string FromSiToStringWithUnits(double value)
    {
        return $"{value * SiInMicroAmp:F1} {Unit}";
    }
}

public class DdmLlzMicroAmpRu : IMeasureUnitItem<double, DdmUnits>
{
    private const double SiInMicroAmpRu = 250 / 0.155;
    public DdmUnits Id => DdmUnits.InParts;
    public string Title => "[DDM]";
    public string Unit => "µA";
    public bool IsSiUnit => false;
    public double ConvertFromSi(double siValue)
    {
        return siValue * SiInMicroAmpRu;
    }

    public double ConvertToSi(double value)
    {
        return value / SiInMicroAmpRu;
    }

    public bool IsValid(string value)
    {
        if (value.IsNullOrWhiteSpace()) return false;
        if (double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
        {
            return v is >= -806.45 and <= 806.45;
        }
        return false;
    }

    public string GetErrorMessage(string value)
    {
        if (value.IsNullOrWhiteSpace()) return "Value can't be null or white space"; // TODO: Localize
        value = value.Replace(',', '.');
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) == false)
            return "Value must be a number";
        return v is >= -806.45 and <= 806.45 ? null : "The value must be greater than -483.87 and less than 483.87";
    }

    public double ConvertToSi(string value)
    {
        if (value.IsNullOrWhiteSpace()) return double.NaN;
        return double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v * SiInMicroAmpRu : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return (value * SiInMicroAmpRu).ToString("F1");
    }

    public string FromSiToStringWithUnits(double value)
    {
        return $"{value * SiInMicroAmpRu:F1} {Unit}";
    }
}