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
        IMeasureUnit<double,AltitudeUnits> Altitude { get; }

        IMeasureUnit<double,DistanceUnits> Distance { get; }

        IMeasureUnit<double,LatitudeLongitudeUnits> LatitudeAndLongitude { get; }
        
        IMeasureUnit<double,VelocityUnits> Velocity { get; }

       
        
        #endregion
    }



    
}