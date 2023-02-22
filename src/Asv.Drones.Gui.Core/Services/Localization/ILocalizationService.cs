﻿using Asv.Common;
using System.Globalization;
using Asv.Avalonia.Map;

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
        IRxEditableValue<LanguageInfo> CurrentLanguage { get; }
        IEnumerable<LanguageInfo> AvailableLanguages { get; }

        string BytesToString(long bytes);
        string RateToString(long bytesPerSec);
    }



    
}