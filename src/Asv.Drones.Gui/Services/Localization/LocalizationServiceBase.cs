using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui
{
    public class LanguageInfo : ILanguageInfo
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

    public class LocalizationServiceConfig
    {
        public string? CurrentLanguage { get; set; }
    }

    [Export]
    public class LocalizationServiceBase : ServiceWithConfigBase<LocalizationServiceConfig>, ILocalizationService
    {
        private readonly List<LanguageInfo> _languages =
        [
            new LanguageInfo("en", "English (EN)", () => CultureInfo.GetCultureInfo("en")),
            new LanguageInfo("ru", "Русский (RU)", () => CultureInfo.GetCultureInfo("ru"))
        ];

        [ImportingConstructor]
        public LocalizationServiceBase(IConfiguration cfgSvc) : base(cfgSvc)
        {
            #region CurrentLanguageInit

            var selectedLang = default(LanguageInfo);
            var langFromConfig = InternalGetConfig(_ => _.CurrentLanguage);
            if (string.IsNullOrWhiteSpace(langFromConfig) == false)
            {
                selectedLang = _languages.FirstOrDefault(_ => _.Id.Equals(langFromConfig));
            }

            selectedLang ??= _languages.First();
            CurrentLanguage = new RxValue<ILanguageInfo>(selectedLang).DisposeItWith(Disposable);
            CurrentLanguage.Subscribe(SetLanguage).DisposeItWith(Disposable);

            #endregion

            Altitude = new Altitude(cfgSvc, nameof(Altitude)).DisposeItWith(Disposable);
            Velocity = new Velocity(cfgSvc, nameof(Velocity)).DisposeItWith(Disposable);
            Distance = new Distance(cfgSvc, nameof(Distance)).DisposeItWith(Disposable);
            Latitude = new Latitude(cfgSvc, nameof(Latitude)).DisposeItWith(Disposable);
            Longitude = new Longitude(cfgSvc, nameof(Longitude)).DisposeItWith(Disposable);

            DdmLlz = new DdmLlz(cfgSvc, nameof(DdmLlz)).DisposeItWith(Disposable);
            DdmGp = new DdmGp(cfgSvc, nameof(DdmLlz)).DisposeItWith(Disposable);
            Sdm = new Sdm(cfgSvc, nameof(Sdm)).DisposeItWith(Disposable);
            Power = new Power(cfgSvc, nameof(Power)).DisposeItWith(Disposable);
            AmplitudeModulation =
                new AmplitudeModulation(cfgSvc, nameof(AmplitudeModulation)).DisposeItWith(Disposable);
            Frequency = new Frequency(cfgSvc, nameof(Frequency)).DisposeItWith(Disposable);
            Phase = new Phase(cfgSvc, nameof(Phase)).DisposeItWith(Disposable);
            Bearing = new Bearing(cfgSvc, nameof(Bearing)).DisposeItWith(Disposable);
            Temperature = new Temperature(cfgSvc, nameof(Temperature)).DisposeItWith(Disposable);
            Degree = new Degrees(cfgSvc, nameof(Degree)).DisposeItWith(Disposable);
            FieldStrength = new FieldStrength(cfgSvc, nameof(FieldStrength)).DisposeItWith(Disposable);
        }

        public IRxEditableValue<ILanguageInfo> CurrentLanguage { get; }
        public IEnumerable<ILanguageInfo> AvailableLanguages => _languages;

        private void SetLanguage(ILanguageInfo lang)
        {
            if (lang == null) throw new ArgumentNullException(nameof(lang));
            if (lang is not LanguageInfo langInfo) throw new ArgumentException("Invalid language info", nameof(lang));
            var culture = langInfo.Culture;
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
        public IReadOnlyMeasureUnit<double> MAh { get; } = new MAh();
        
        public IMeasureUnit<double, AltitudeUnits> Altitude { get; }
        public IMeasureUnit<double, DistanceUnits> Distance { get; }
        public IMeasureUnit<double, LatitudeUnits> Latitude { get; }
        public IMeasureUnit<double, LongitudeUnits> Longitude { get; }
        public IMeasureUnit<double, VelocityUnits> Velocity { get; }
        public IMeasureUnit<double, DdmUnits> DdmLlz { get; }
        public IMeasureUnit<double, DdmUnits> DdmGp { get; }
        public IMeasureUnit<double, SdmUnits> Sdm { get; }
        public IMeasureUnit<double, PowerUnits> Power { get; }
        public IMeasureUnit<double, AmplitudeModulationUnits> AmplitudeModulation { get; }
        public IMeasureUnit<double, FrequencyUnits> Frequency { get; }
        public IMeasureUnit<double, PhaseUnits> Phase { get; }
        public IMeasureUnit<double, BearingUnits> Bearing { get; }
        public IMeasureUnit<double, TemperatureUnits> Temperature { get; }
        public IMeasureUnit<double, DegreeUnits> Degree { get; }
        public IMeasureUnit<double, FieldStrengthUnits> FieldStrength { get; }

        #endregion
    }
}