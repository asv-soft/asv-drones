using Asv.Common;
using System.Globalization;

namespace Asv.Drones.Gui.Core
{
    public class LanguageInfo
    {
        private CultureInfo? _culture;
        private readonly Func<CultureInfo> _getCulture;

        public LanguageInfo(string id, string displayName, Func<CultureInfo> getCulture)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(displayName));
            Id = id;
            DisplayName = displayName;
            _getCulture = getCulture ?? throw new ArgumentNullException(nameof(getCulture));
        }

        public string Id { get; }
        public string DisplayName { get; }
        public CultureInfo Culture => _culture ??= _getCulture();
    }

    public interface ILocalizationService
    {
        /// <summary>
        /// Allows you to select or get the current application language
        /// </summary>
        IRxEditableValue<LanguageInfo> CurrentLanguage { get; }
        /// <summary>
        /// Returns the list of available languages
        /// </summary>
        IEnumerable<LanguageInfo> AvailableLanguages { get; }
        
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

        IMeasureUnit<double,AltitudeUnits> Altitude { get; }

        IMeasureUnit<double,DistanceUnits> Distance { get; }

        IMeasureUnit<double,LatitudeUnits> Latitude { get; }
        IMeasureUnit<double,LongitudeUnits> Longitude { get; }

        IMeasureUnit<double,VelocityUnits> Velocity { get; }
        
        IMeasureUnit<double,DdmUnits> DdmLlz { get; }
        IMeasureUnit<double,DdmUnits> DdmGp { get; }

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
            return new GeoPoint(lat,lon,alt);
        }
    }
    
    



    
}