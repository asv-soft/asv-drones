using Asv.Common;
using System.Globalization;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageInfo"/> class.
    /// </summary>
    /// <param name="id">The identifier of the language.</param>
    /// <param name="displayName">The display name of the language.</param>
    /// <param name="getCulture">The function to retrieve the <see cref="CultureInfo"/> object for the language.</param>
    /// <exception cref="ArgumentException">Thrown when id or displayName is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when getCulture is null.</exception>
    public class LanguageInfo
    {
        /// <summary>
        /// The culture information used for formatting and parsing values.
        /// </summary>
        /// <remarks>
        /// This variable holds an instance of <see cref="CultureInfo"/> which provides information about
        /// a specific culture, including the language, writing system, calendar, and cultural conventions.
        /// The value can be null if no specific culture is set.
        /// </remarks>
        private CultureInfo? _culture;

        /// <summary>
        /// Gets the culture information.
        /// </summary>
        /// <returns>
        /// The culture information.
        /// </returns>
        /// <remarks>
        /// This is a delegate that should return the culture information.
        /// </remarks>
        private readonly Func<CultureInfo> _getCulture;

        /// <summary>
        /// Represents information about a language.
        /// </summary>
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

        /// <summary>
        /// Gets the unique identifier for the property.
        /// </summary>
        /// <value>
        /// The unique identifier for the property.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets the display name of the object.
        /// </summary>
        /// <remarks>
        /// The display name represents the name or title that is shown to the user for identification
        /// or labeling purposes. It does not necessarily have to be unique within a system.
        /// </remarks>
        /// <value>
        /// A string representing the display name of the object.
        /// </value>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the CultureInfo of the property.
        /// </summary>
        /// <remarks>
        /// This property returns the CultureInfo associated with the current instance.
        /// If the CultureInfo is not set, it will be lazily initialized using the private _getCulture() method.
        /// </remarks>
        /// <value>
        /// The CultureInfo of the property.
        /// </value>
        public CultureInfo Culture => _culture ??= _getCulture();
    }

    /// <summary>
    /// Represents a localization service for handling language and unit conversions.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets or sets the current application language.
        /// </summary>
        /// <remarks>
        /// This property allows you to select or get the current application language.
        /// </remarks>
        /// <value>
        /// An instance of <see cref="LanguageInfo"/> representing the current language.
        /// </value>
        IRxEditableValue<LanguageInfo> CurrentLanguage { get; }

        /// <summary>
        /// Gets the list of available languages.
        /// </summary>
        /// <value>
        /// An enumerable collection of LanguageInfo objects representing the available languages.
        /// </value>
        IEnumerable<LanguageInfo> AvailableLanguages { get; }

        #region Units

        /// <summary>
        /// Gets the byte rate as a short localized string.
        /// </summary>
        /// <remarks>
        /// Use this property to convert the byte rate to a string representation with the appropriate unit.
        /// For example: 1024 => "1 KB/s"
        /// </remarks>
        /// <value>
        /// An object implementing the <see cref="IReadOnlyMeasureUnit{T}"/> interface, where T is double.
        /// </value>
        IReadOnlyMeasureUnit<double> ByteRate { get; }

        /// <summary>
        /// Gets the items rate as a short localized string.
        /// For example: 1000 => 1 KHz
        /// </summary>
        /// <returns>A read-only measure unit with a double value representing the items rate.</returns>
        IReadOnlyMeasureUnit<double> ItemsRate { get; }

        /// <summary>
        /// Gets the measure unit for byte sizes, represented as a short localized string.
        /// For example, 1024 bytes would be represented as "1 KB".
        /// </summary>
        /// <returns>The measure unit for byte sizes.</returns>
        IReadOnlyMeasureUnit<long> ByteSize { get; }

        /// <summary>
        /// Gets the read-only measure unit for relative time.
        /// </summary>
        /// <value>
        /// The read-only measure unit for relative time.
        /// </value>
        IReadOnlyMeasureUnit<TimeSpan> RelativeTime { get; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        IReadOnlyMeasureUnit<double> Voltage { get; }

        /// <summary>
        /// Gets the current measurement unit.
        /// </summary>
        /// <returns>
        /// The current measurement unit.
        /// </returns>
        IReadOnlyMeasureUnit<double> Current { get; }

        /// <summary>
        /// Gets the altitude value with its associated units.
        /// </summary>
        /// <value>
        /// The altitude value with its associated units.
        /// </value>
        IMeasureUnit<double,AltitudeUnits> Altitude { get; }

        /// <summary>
        /// Gets the distance measurement.
        /// </summary>
        /// <value>
        /// The distance measurement.
        /// </value>
        IMeasureUnit<double,DistanceUnits> Distance { get; }

        /// <summary>
        /// Gets the latitude as a measurement with a specified unit of measure.
        /// </summary>
        /// <returns>The latitude measurement.</returns>
        IMeasureUnit<double,LatitudeUnits> Latitude { get; }

        /// <summary>
        /// Gets the longitude measurement unit.
        /// </summary>
        /// <value>
        /// The longitude measurement unit.
        /// </value>
        IMeasureUnit<double,LongitudeUnits> Longitude { get; }

        /// <summary>
        /// Gets the velocity value with its associated measurement unit.
        /// </summary>
        /// <value>
        /// The velocity value with its associated measurement unit.
        /// </value>
        IMeasureUnit<double,VelocityUnits> Velocity { get; }

        /// <summary>
        /// Gets the measurement unit for DdmLlz property.
        /// </summary>
        /// <value>
        /// The measurement unit for DdmLlz property.
        /// </value>
        IMeasureUnit<double,DdmUnits> DdmLlz { get; }

        /// <summary>
        /// Gets the measurement unit for DdmGp.
        /// </summary>
        /// <value>The measurement unit.</value>
        IMeasureUnit<double,DdmUnits> DdmGp { get; }

        /// Gets or sets the measurement unit for the Sdm property.
        /// </summary>
        /// <value>
        /// The measurement unit for the Sdm property.
        /// </value>
        IMeasureUnit<double, SdmUnits> Sdm { get; }

        /// <summary>
        /// Represents the power property.
        /// </summary>
        /// <returns>
        /// The power property as an instance of IMeasureUnit<double, PowerUnits>.
        /// </returns>
        IMeasureUnit<double, PowerUnits> Power { get; }

        /// <summary>
        /// Gets the amplitude modulation measurement.
        /// </summary>
        /// <value>
        /// The amplitude modulation measurement.
        /// </value>
        IMeasureUnit<double, AmplitudeModulationUnits> AmplitudeModulation { get; }

        /// <summary>
        /// Represents the property for frequency measurements.
        /// </summary>
        /// <value>
        /// The property for frequency measurements.
        /// </value>
        IMeasureUnit<double, FrequencyUnits> Frequency { get; }

        /// <summary>
        /// Gets the phase measurement.
        /// </summary>
        /// <remarks>
        /// The <see cref="Phase"/> property represents a measurement of phase.
        /// </remarks>
        /// <returns>
        /// The phase measurement.
        /// </returns>
        IMeasureUnit<double, PhaseUnits> Phase { get; }

        /// <summary>
        /// Property that represents a bearing.
        /// </summary>
        /// <value>
        /// An object of type <see cref="IMeasureUnit{T, U}"/> where T is <see cref="double"/>
        /// and U is <see cref="BearingUnits"/>.
        /// </value>
        /// <remarks>
        /// The bearing is a measurement of the horizontal direction of an object, relative to a reference point.
        /// It is usually represented in degrees or radians.
        /// </remarks>
        IMeasureUnit<double, BearingUnits> Bearing { get; }

        /// <summary>
        /// Represents a property that represents temperature measurement.
        /// </summary>
        /// <value>
        /// The temperature measurement represented using the <see cref="IMeasureUnit{T, TUnit}"/> interface.
        /// </value>
        IMeasureUnit<double, TemperatureUnits> Temperature { get; }

        /// <summary>
        /// Represents a measurement in degrees.
        /// </summary>
        /// <seealso cref="IMeasureUnit{T,U}"/>
        IMeasureUnit<double, DegreeUnits> Degree { get; }

        /// <summary>
        /// Gets the field strength value.
        /// </summary>
        /// <value>
        /// The field strength value.
        /// </value>
        /// <remarks>
        /// The field strength is measured in units of FieldStrengthUnits.
        /// </remarks>
        IMeasureUnit<double, FieldStrengthUnits> FieldStrength { get; }

        #endregion

        /// <summary>
        /// Converts the latitude, longitude, and altitude values to a GeoPoint object in
        /// the International System of Units.
        /// </summary>
        /// <param name="latitude">The latitude value in degrees or DMS format.
        /// If the latitude is invalid, it defaults to double.NaN.</param>
        /// <param name="longitude">The longitude value in degrees or DMS format.
        /// If the longitude is invalid, it defaults to double.NaN.</param>
        /// <param name="altitude">The altitude value in meters or feet.
        /// If the altitude is invalid, it defaults to double.NaN.</param>
        /// <returns>A GeoPoint object representing the converted latitude, longitude, and altitude
        /// values in the International System of Units.</returns>
        public GeoPoint ToSiGeoPoint(string? latitude, string? longitude, string? altitude)
        {
            var lat = Latitude.IsValid(latitude) ? Latitude.ConvertToSi(latitude) : double.NaN;
            var lon = Longitude.IsValid(longitude) ? Longitude.ConvertToSi(longitude) : double.NaN;
            var alt = Altitude.IsValid(altitude) ? Altitude.ConvertToSi(altitude) : double.NaN;
            return new GeoPoint(lat,lon,alt);
        }
    }
    
    
    
    



    
}