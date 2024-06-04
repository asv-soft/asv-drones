using Asv.Common;

namespace Asv.Drones.Gui.Api
{
    public interface ILocalizationService
    {
        /// <summary>
        /// Allows you to select or get the current application language
        /// </summary>
        IRxEditableValue<ILanguageInfo> CurrentLanguage { get; }

        /// <summary>
        /// Returns the list of available languages
        /// </summary>
        IEnumerable<ILanguageInfo> AvailableLanguages { get; }

        #region Units

        /// <summary>
        /// Convert bytes rate to short localized string
        /// For example: 1024 => 1 KB/s
        /// </summary>
        /// <returns></returns>
        IReadOnlyMeasureUnit<double> ByteRate { get; }

        /// <summary>
        /// Convert items rate to short localized string
        /// For example: 1000 => 1 KHz
        /// </summary>
        /// <returns></returns>
        IReadOnlyMeasureUnit<double> ItemsRate { get; }

        /// <summary>
        /// Convert bytes count to short localized string
        /// For example: 1024 => 1 KB
        /// </summary>
        /// <returns></returns>
        IReadOnlyMeasureUnit<long> ByteSize { get; }

        IReadOnlyMeasureUnit<TimeSpan> RelativeTime { get; }

        IReadOnlyMeasureUnit<double> Voltage { get; }

        IReadOnlyMeasureUnit<double> Current { get; }
        IReadOnlyMeasureUnit<double> MAh { get; }

        IMeasureUnit<double, AltitudeUnits> Altitude { get; }

        IMeasureUnit<double, DistanceUnits> Distance { get; }
        
        IMeasureUnit<double, DistanceUnits> Accuracy { get; } // field for gbs plugin only

        IMeasureUnit<double, LatitudeUnits> Latitude { get; }
        IMeasureUnit<double, LongitudeUnits> Longitude { get; }

        IMeasureUnit<double, VelocityUnits> Velocity { get; }

        IMeasureUnit<double, DdmUnits> DdmLlz { get; }
        IMeasureUnit<double, DdmUnits> DdmGp { get; }

        IMeasureUnit<double, SdmUnits> Sdm { get; }

        IMeasureUnit<double, PowerUnits> Power { get; }
        IMeasureUnit<double, AmplitudeModulationUnits> AmplitudeModulation { get; }
        IMeasureUnit<double, FrequencyUnits> Frequency { get; }
        IMeasureUnit<double, PhaseUnits> Phase { get; }
        IMeasureUnit<double, BearingUnits> Bearing { get; }
        IMeasureUnit<double, TemperatureUnits> Temperature { get; }
        IMeasureUnit<double, DegreeUnits> Degree { get; }
        IMeasureUnit<double, FieldStrengthUnits> FieldStrength { get; }

        #endregion

        public GeoPoint ToSiGeoPoint(string? latitude, string? longitude, string? altitude)
        {
            var lat = Latitude.IsValid(latitude) ? Latitude.ConvertToSi(latitude) : double.NaN;
            var lon = Longitude.IsValid(longitude) ? Longitude.ConvertToSi(longitude) : double.NaN;
            var alt = Altitude.IsValid(altitude) ? Altitude.ConvertToSi(altitude) : double.NaN;
            return new GeoPoint(lat, lon, alt);
        }
    }

    public interface ILanguageInfo
    {
        string Id { get; }
        string DisplayName { get; }
    }

    public enum AltitudeUnits
    {
        Meters,
        Feets
    }

    public enum AmplitudeModulationUnits
    {
        Percent,
        InParts
    }

    public enum BearingUnits
    {
        Degree,
        DegreesMinutes
    }

    public enum DdmUnits
    {
        InParts,
        Percent,
        MicroAmp,
        MicroAmpRu
    }

    public enum DegreeUnits
    {
        Degrees,
        MinutesSeconds,
        DegreesMinutesSeconds
    }

    public enum DistanceUnits
    {
        Meters,
        NauticalMiles
    }

    public enum FieldStrengthUnits
    {
        MicroVoltsPerMeter
    }

    public enum FrequencyUnits
    {
        Hz,
        KHz,
        MHz,
        GHz
    }

    public enum LatitudeUnits
    {
        Deg,
        Dms
    }

    public enum LongitudeUnits
    {
        Deg,
        Dms
    }

    public enum PhaseUnits
    {
        Degree,
        Radian
    }

    public enum PowerUnits
    {
        Dbm
    }

    public enum SdmUnits
    {
        Percent
    }

    public enum TemperatureUnits
    {
        Celsius,
        Farenheit,
        Kelvin
    }

    public enum VelocityUnits
    {
        MetersPerSecond,
        KilometersPerHour,
        MilesPerHour
    }
}