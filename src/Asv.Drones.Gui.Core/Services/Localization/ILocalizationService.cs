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

    public enum AltitudeUnits
    {
        Meters,
        Feets
    }

    public enum DistanceUnits
    {
        Meters,
        NauticalMiles
    }

    public enum LatitudeLongitudeUnits
    {
        Degrees,
        DegreesMinutesSeconds
    }

    public class AltitudeUnitItem
    {
        public AltitudeUnitItem(AltitudeUnits id, string name)
        {
            Id = id;
            Name = name;
        }

        public AltitudeUnits Id { get; }
        public string Name { get; }
    }

    public class DistanceUnitItem
    {
        public DistanceUnitItem(DistanceUnits id, string name)
        {
            Id = id;
            Name = name;
        }

        public DistanceUnits Id { get; }
        public string Name { get; }
    }

    public class LatitudeLongitudeUnitItem
    {
        public LatitudeLongitudeUnitItem(LatitudeLongitudeUnits id, string name)
        {
            Id = id;
            Name = name;
        }

        public LatitudeLongitudeUnits Id { get; }
        public string Name { get; }
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
        
        IRxEditableValue<AltitudeUnitItem> CurrentAltitudeUnit { get; }
        IEnumerable<AltitudeUnitItem> AltitudeUnits { get; }

        IRxEditableValue<DistanceUnitItem> CurrentDistanceUnit { get; }
        IEnumerable<DistanceUnitItem> DistanceUnits { get; }
        
        IRxEditableValue<LatitudeLongitudeUnitItem> CurrentLatitudeLongitudeUnit { get; }
        IEnumerable<LatitudeLongitudeUnitItem> LatitudeLongitudeUnits { get; }
        
        #region Units

        /// <summary>
        /// Convert bytes rate to short localized string
        /// For example: 1024 => 1 KB/s
        /// </summary>
        /// <returns></returns>
        IMeasureUnit<double> ByteRate { get; }

        /// <summary>
        /// Convert items rate to short localized string
        /// For example: 1000 => 1 KHz
        /// </summary>
        /// <returns></returns>
        IMeasureUnit<double> ItemsRate { get; }

        /// <summary>
        /// Convert bytes count to short localized string
        /// For example: 1024 => 1 KB
        /// </summary>
        /// <returns></returns>
        IMeasureUnit<long> ByteSize { get; }

        IMeasureUnit<double> Altitude { get; }

        IMeasureUnit<double> Distance { get; }

        IMeasureUnit<double> LatitudeAndLongitude { get; }
        #endregion
    }



    
}