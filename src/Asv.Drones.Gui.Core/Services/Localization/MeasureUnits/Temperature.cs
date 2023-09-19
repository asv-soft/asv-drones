using System.Globalization;
using System.Reactive;
using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum TemperatureUnits
{
    Celsius,
    Farenheit,
    Kelvin
}
public class Temperature : MeasureUnitBase<double, TemperatureUnits>
{
    private static readonly IMeasureUnitItem<double, TemperatureUnits>[] _units = {
        new TemperatureCelsius(),
        new TemperatureFarenheit(),
        new DoubleMeasureUnitItem<TemperatureUnits>(TemperatureUnits.Kelvin,RS.Temperature_Kelvin_Title,"K",true,"F1",1),
    };
    
    public Temperature(IConfiguration cfgSvc, string cfgKey):base(cfgSvc,cfgKey,_units)
    {
        
    }
    
    public override string Title => RS.MeasureUnitsSettingsViewModelTemperature;
    public override string Description => RS.MeasureUnitsSettingsViewModelTemperatureDescription;
}

public class TemperatureCelsius : IMeasureUnitItem<double, TemperatureUnits>
{
    private const double ZeroCelsiusInKelvin = 273.15;
    public TemperatureUnits Id => TemperatureUnits.Celsius;
    public string Title => RS.Temperature_Celsius_Title;
    public string Unit => "°C";
    public bool IsSiUnit => false;
    public double ConvertFromSi(double siValue)
    {
        return siValue - ZeroCelsiusInKelvin;
    }

    public double ConvertToSi(double value)
    {
        return value + ZeroCelsiusInKelvin;
    }

    public double Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return double.NaN;
        return double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : double.NaN;
    }

    public bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) == false)
            return false;
        return true;
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return RS.TemperatureCelsius_ErrorMessage_NullOrWhiteSpace;
        value = value.Replace(',', '.');
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) == false)
            return RS.TemperatureCelsius_ErrorMessage_NaN;
        return null;
    }

    public string Print(double value)
    {
        return $"{value:F1}";
    }
    public string PrintWithUnits(double value)
    {
        return $"{value:F1} {Unit}";
    }
}

public class TemperatureFarenheit : IMeasureUnitItem<double, TemperatureUnits>
{
    private const double ZeroCelsiusInKelvin = 273.15;
    private const double ZeroCelsiusInFarenheit = 32;
    public TemperatureUnits Id => TemperatureUnits.Farenheit;
    public string Title => RS.Temperature_Farenheit_Title;
    public string Unit => "°F";
    public bool IsSiUnit => false;
    public double ConvertFromSi(double siValue)
    {
        return (siValue - ZeroCelsiusInKelvin) * (9.0 / 5.0) + ZeroCelsiusInFarenheit;
    }

    public double ConvertToSi(double value)
    {
        return (value - ZeroCelsiusInFarenheit) * (5.0 / 9.0) + ZeroCelsiusInKelvin;
    }

    public double Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return double.NaN;
        return double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : double.NaN;
    }

    public bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v);
    }

    public string? GetErrorMessage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return RS.TemperatureFarenheit_ErrorMessage_NullOrWhiteSpace;
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) == false ? RS.TemperatureFarenheit_ErrorMessage_NaN : default;
    }

    public double ConvertToSi(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return double.NaN;
        if (double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
        {
            return v + ZeroCelsiusInKelvin;
        }
        return double.NaN;
    }

    public string Print(double value)
    {
        return value.ToString("F1");
    }

    public string PrintWithUnits(double value)
    {
        return $"{value:F1} {Unit}";
    }

}