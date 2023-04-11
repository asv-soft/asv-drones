using System.ComponentModel.Composition;
using System.Globalization;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls.Mixins;

namespace Asv.Drones.Gui.Core
{
    public class LocalizationServiceConfig
    {
        public string? CurrentLanguage { get; set; }
    }

    public class LocalizationServiceBase : ServiceWithConfigBase<LocalizationServiceConfig>, ILocalizationService
    {
        private readonly List<LanguageInfo> _languages = new()
        {
            new LanguageInfo("en","English (EN)", ()=>CultureInfo.GetCultureInfo("en") ),
            new LanguageInfo("ru","Русский (RU)", ()=>CultureInfo.GetCultureInfo("ru") ),
        };

        [ImportingConstructor]
        public LocalizationServiceBase(IConfiguration cfgSvc):base(cfgSvc)
        {
            #region CurrentLanguageInit
            
            var selectedLang = default(LanguageInfo);
            var langFromConfig = InternalGetConfig(_ => _.CurrentLanguage);
            if (string.IsNullOrWhiteSpace(langFromConfig) == false)
            {
                selectedLang = _languages.FirstOrDefault(_ => _.Id.Equals(langFromConfig));
            }

            selectedLang ??= _languages.First();
            CurrentLanguage = new RxValue<LanguageInfo>(selectedLang).DisposeItWith(Disposable);
            CurrentLanguage.Subscribe(SetLanguage).DisposeItWith(Disposable);
            
            #endregion

            Altitude = new Altitude(cfgSvc, nameof(Altitude)).DisposeItWith(Disposable);
            Velocity = new Velocity(cfgSvc, nameof(Velocity)).DisposeItWith(Disposable);
            Distance = new Distance(cfgSvc, nameof(Distance)).DisposeItWith(Disposable);
            Latitude = new Latitude(cfgSvc, nameof(Latitude)).DisposeItWith(Disposable);
            Longitude = new Longitude(cfgSvc, nameof(Longitude)).DisposeItWith(Disposable);
        }

        public IRxEditableValue<LanguageInfo> CurrentLanguage { get; }
        public IEnumerable<LanguageInfo> AvailableLanguages => _languages;

        private void SetLanguage(LanguageInfo lang)
        {
            if (lang == null) throw new ArgumentNullException(nameof(lang));
            var culture = lang.Culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            InternalSaveConfig(_ => _.CurrentLanguage = lang.Id);
        }

        #region Units

        public IReadOnlyMeasureUnit<double> ByteRate { get; } = new BytesRate();
        public IReadOnlyMeasureUnit<double> ItemsRate { get; } = new ItemsRate();
        public IReadOnlyMeasureUnit<long> ByteSize { get; } = new ByteSize();
        public IReadOnlyMeasureUnit<TimeSpan> RelativeTime { get; } = new RelativeTime();
        public IReadOnlyMeasureUnit<double> Voltage { get; } = new Voltage();
        public IReadOnlyMeasureUnit<double> Current { get; } = new Current();

        public IMeasureUnit<double, AltitudeUnits> Altitude { get; }
        public IMeasureUnit<double, DistanceUnits> Distance { get; }
        public IMeasureUnit<double, LatitudeUnits> Latitude { get; }
        public IMeasureUnit<double, LongitudeUnits> Longitude { get; }
        public IMeasureUnit<double, VelocityUnits> Velocity { get; }

        #endregion

    }
}