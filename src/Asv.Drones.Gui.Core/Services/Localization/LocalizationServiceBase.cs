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
        
        public AltitudeUnitItem? CurrentAltitudeUnit { get; set; }

        public DistanceUnitItem? CurrentDistanceUnit { get; set; }

        public LatitudeLongitudeUnitItem? CurrentLatitudeLongitudeUnit { get; set; }
        
        public VelocityUnitItem? CurrentVelocityUnit { get; set; }
    }

    public class LocalizationServiceBase : ServiceWithConfigBase<LocalizationServiceConfig>, ILocalizationService
    {
        private readonly List<LanguageInfo> _languages = new()
        {
            new LanguageInfo("en","English (EN)", ()=>CultureInfo.GetCultureInfo("en") ),
            new LanguageInfo("ru","Русский (RU)", ()=>CultureInfo.GetCultureInfo("ru") ),
        };

        private readonly List<AltitudeUnitItem> _altitudes;

        private readonly List<DistanceUnitItem> _distances;

        private readonly List<LatitudeLongitudeUnitItem> _latitudesAndLongitudes;

        private readonly List<VelocityUnitItem> _velocities;

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

            #region CurrentAltitudeUnitInit
            _altitudes = new()
            {
                new(Core.AltitudeUnits.Meters, RS.SettingsThemeViewModel_AltitudeMeters),
                new(Core.AltitudeUnits.Feets, RS.SettingsThemeViewModel_AltitudeFeets)
            };

            var selectedAltitude = _altitudes.First();
            var altitudeFromConfig = InternalGetConfig(_ => _.CurrentAltitudeUnit);

            if (string.IsNullOrWhiteSpace(altitudeFromConfig?.Name) == false)
            {
                selectedAltitude = _altitudes.FirstOrDefault(_ => _.Id.Equals(altitudeFromConfig.Id));
            }

            CurrentAltitudeUnit = new RxValue<AltitudeUnitItem>(selectedAltitude).DisposeItWith(Disposable);
            CurrentAltitudeUnit.Subscribe(SetAltitudeUnit).DisposeWith(Disposable);
            #endregion

            #region CurrentDistanceUnitInit
            _distances = new()
            {
                new (Core.DistanceUnits.Meters, RS.SettingsThemeViewModel_DistanceMeters),
                new (Core.DistanceUnits.NauticalMiles, RS.SettingsThemeViewModel_DistanceInternationalNauticalMiles)
            };

            var selectedDistance = _distances.First();
            var distanceFromConfig = InternalGetConfig(_ => _.CurrentDistanceUnit);

            if (string.IsNullOrWhiteSpace(distanceFromConfig?.Name) == false)
            {
                selectedDistance = _distances.FirstOrDefault(_ => _.Id.Equals(distanceFromConfig.Id));
            }

            CurrentDistanceUnit = new RxValue<DistanceUnitItem>(selectedDistance).DisposeItWith(Disposable);
            CurrentDistanceUnit.Subscribe(SetDistanceUnit).DisposeWith(Disposable);
            #endregion

            #region CurrentLatitudeLongitudeUnitInit
            _latitudesAndLongitudes = new()
            {
                new ( Core.LatitudeLongitudeUnits.Degrees, RS.SettingsThemeViewModel_LatitudeLongtitudeDegrees),
                new (Core.LatitudeLongitudeUnits.DegreesMinutesSeconds, RS.SettingsThemeViewModel_LatitudeLongtitudeDegreesMinutesSeconds)
            };

            var selectedLatitudeLongitude = _latitudesAndLongitudes.First();
            var latitudeLongitudeFromConfig = InternalGetConfig(_ => _.CurrentLatitudeLongitudeUnit);

            if (string.IsNullOrWhiteSpace(latitudeLongitudeFromConfig?.Name) == false)
            {
                selectedLatitudeLongitude = _latitudesAndLongitudes.FirstOrDefault(_ => _.Id.Equals(latitudeLongitudeFromConfig.Id));
            }

            CurrentLatitudeLongitudeUnit = new RxValue<LatitudeLongitudeUnitItem>(selectedLatitudeLongitude).DisposeItWith(Disposable);
            CurrentLatitudeLongitudeUnit.Subscribe(SetLatitudeLongitudeUnit).DisposeWith(Disposable);
            #endregion

            #region CurrentVelocityUnitInit
            _velocities = new()
            {
                new ( Core.VelocityUnits.MetersPerSecond, RS.SettingsThemeViewModel_VelocityMetersPerSecondUnit),
                new (Core.VelocityUnits.KilometersPerHour, RS.SettingsThemeViewModel_VelocityKilometersPerHourUnit),
                new (Core.VelocityUnits.MilesPerHour, RS.SettingsThemeViewModel_VelocityMilesPerHourUnit)
            };

            var selectedVelocity = _velocities.First();
            var velocityFromConfig = InternalGetConfig(_ => _.CurrentVelocityUnit);

            if (string.IsNullOrWhiteSpace(velocityFromConfig?.Name) == false)
            {
                selectedVelocity = _velocities.FirstOrDefault(_ => _.Id.Equals(velocityFromConfig.Id));
            }

            CurrentVelocityUnit = new RxValue<VelocityUnitItem>(selectedVelocity).DisposeItWith(Disposable);
            CurrentVelocityUnit.Subscribe(SetVelocityUnit).DisposeWith(Disposable);
            #endregion
        }

        public IRxEditableValue<LanguageInfo> CurrentLanguage { get; }
        public IEnumerable<LanguageInfo> AvailableLanguages => _languages;

        public IRxEditableValue<AltitudeUnitItem> CurrentAltitudeUnit { get; }
        public IEnumerable<AltitudeUnitItem> AltitudeUnits => _altitudes;

        public IRxEditableValue<DistanceUnitItem> CurrentDistanceUnit { get; }
        public IEnumerable<DistanceUnitItem> DistanceUnits => _distances;

        public IRxEditableValue<LatitudeLongitudeUnitItem> CurrentLatitudeLongitudeUnit { get; }
        public IEnumerable<LatitudeLongitudeUnitItem> LatitudeLongitudeUnits => _latitudesAndLongitudes;

        public IRxEditableValue<VelocityUnitItem> CurrentVelocityUnit { get; }
        public IEnumerable<VelocityUnitItem> VelocityUnits => _velocities;

        
        private void SetLanguage(LanguageInfo lang)
        {
            if (lang == null) throw new ArgumentNullException(nameof(lang));
            var culture = lang.Culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            InternalSaveConfig(_ => _.CurrentLanguage = lang.Id);
        }

        private void SetAltitudeUnit(AltitudeUnitItem altitude)
        {
            if(altitude == null) throw new ArgumentNullException(nameof(altitude));
            Altitude = new Altitude(altitude.Id);
            InternalSaveConfig(_ => _.CurrentAltitudeUnit = altitude);
        }

        private void SetDistanceUnit(DistanceUnitItem distance)
        {
            if (distance == null) throw new ArgumentNullException(nameof(distance));
            Distance = new Distance(distance.Id);
            InternalSaveConfig(_ => _.CurrentDistanceUnit = distance);
        }

        private void SetLatitudeLongitudeUnit(LatitudeLongitudeUnitItem latitudeLongitude)
        {
            if (latitudeLongitude == null) throw new ArgumentNullException(nameof(latitudeLongitude));
            LatitudeAndLongitude = new LatitudeAndLongitude(latitudeLongitude.Id);
            InternalSaveConfig(_ => _.CurrentLatitudeLongitudeUnit = latitudeLongitude);
        }

        private void SetVelocityUnit(VelocityUnitItem velocity)
        {
            if (velocity == null) throw new ArgumentNullException(nameof(velocity));
            Velocity = new Velocity(velocity.Id);
            InternalSaveConfig(_ => _.CurrentVelocityUnit = velocity);
        }
        
        #region Units

        public IMeasureUnit<double> ByteRate { get; } = new BytesRate();

        public IMeasureUnit<double> ItemsRate { get; } = new ItemsRate();

        public IMeasureUnit<long> ByteSize { get; } = new ByteSize();

        public IMeasureUnit<double> Altitude { get; private set; }

        public IMeasureUnit<double> Distance { get; private set; }

        public IMeasureUnit<double> LatitudeAndLongitude { get; private set; }
        
        public IMeasureUnit<double> Velocity { get; private set; }

        #endregion

    }
}