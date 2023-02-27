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
        /// Convert bytes count to short localized string
        /// For example: 1024 => 1 KB
        /// </summary>
        /// <param name="bytes">Total bytes</param>
        /// <returns></returns>
        string BytesToString(long bytes);
        /// <summary>
        /// Convert bytes rate to short localized string
        /// For example: 1024 => 1 KB/s
        /// </summary>
        /// <param name="bytesPerSec">Total bytes</param>
        /// <returns></returns>
        string BytesRateToString(long bytesPerSec);

        #endregion
    }



    
}