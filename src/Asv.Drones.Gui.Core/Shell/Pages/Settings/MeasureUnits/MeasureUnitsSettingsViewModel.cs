using System.ComponentModel.Composition;
using Asv.Common;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(ISettingsPart))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MeasureUnitsSettingsViewModel : SettingsPartBase
    {
        private static readonly Uri Uri = new(SettingsPartBase.Uri, "measureunits");
        
        private readonly ILocalizationService _localization;
        
        public override int Order => 2;
        
        public MeasureUnitsSettingsViewModel() : base(Uri)
        {
            
        }

        [ImportingConstructor]
        public MeasureUnitsSettingsViewModel(ILocalizationService localization) : this()
        {
            _localization = localization;
            
            _localization.Altitude.CurrentUnit.Subscribe(_ => SelectedAltitudeUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedAltitudeUnit)
                .Subscribe(_localization.Altitude.CurrentUnit)
                .DisposeItWith(Disposable);
            
            _localization.Distance.CurrentUnit.Subscribe(_ => SelectedDistanceUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedDistanceUnit)
                .Subscribe(_localization.Distance.CurrentUnit)
                .DisposeItWith(Disposable);

            _localization.Latitude.CurrentUnit.Subscribe(_ => SelectedLatitudeUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedLatitudeUnit)
                .Subscribe( _localization.Latitude.CurrentUnit)
                .DisposeItWith(Disposable);
            
            _localization.Longitude.CurrentUnit.Subscribe(_ => SelectedLongitudeUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedLongitudeUnit)
                .Subscribe( _localization.Longitude.CurrentUnit)
                .DisposeItWith(Disposable);
            
            _localization.Velocity.CurrentUnit.Subscribe(_ => SelectedVelocityUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedVelocityUnit)
                .Subscribe(_localization.Velocity.CurrentUnit)
                .DisposeItWith(Disposable);
            
            _localization.DdmLlz.CurrentUnit.Subscribe(_ => SelectedDdmLlzUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedDdmLlzUnit)
                .Subscribe(_localization.DdmLlz.CurrentUnit)
                .DisposeItWith(Disposable);
            
            _localization.DdmGp.CurrentUnit.Subscribe(_ => SelectedDdmGpUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedDdmGpUnit)
                .Subscribe(_localization.DdmGp.CurrentUnit)
                .DisposeItWith(Disposable);
            
            _localization.Temperature.CurrentUnit.Subscribe(_ => SelectedTemperatureUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedTemperatureUnit)
                .Subscribe(_localization.Temperature.CurrentUnit)
                .DisposeItWith(Disposable);
        }
        
        [Reactive]
        public IMeasureUnitItem<double,AltitudeUnits> SelectedAltitudeUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,AltitudeUnits>> AltitudeUnits => _localization.Altitude.AvailableUnits;

        [Reactive]
        public IMeasureUnitItem<double,DistanceUnits> SelectedDistanceUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,DistanceUnits>> DistanceUnits => _localization.Distance.AvailableUnits;

        [Reactive]
        public IMeasureUnitItem<double,LatitudeUnits> SelectedLatitudeUnit { get; set; }
        public IEnumerable<IMeasureUnitItem<double,LatitudeUnits>> LatitudeUnits => _localization.Latitude.AvailableUnits;
        
        [Reactive]
        public IMeasureUnitItem<double,LongitudeUnits> SelectedLongitudeUnit { get; set; }
        public IEnumerable<IMeasureUnitItem<double,LongitudeUnits>> LongitudeUnits => _localization.Longitude.AvailableUnits;
        
        [Reactive]
        public IMeasureUnitItem<double,VelocityUnits> SelectedVelocityUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,VelocityUnits>> VelocityUnits => _localization.Velocity.AvailableUnits;
        
        [Reactive]
        public IMeasureUnitItem<double,DdmUnits> SelectedDdmLlzUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,DdmUnits>> DdmLlzUnits => _localization.DdmLlz.AvailableUnits;
        
        [Reactive]
        public IMeasureUnitItem<double,DdmUnits> SelectedDdmGpUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,DdmUnits>> DdmGpUnits => _localization.DdmGp.AvailableUnits;
        
        [Reactive]
        public IMeasureUnitItem<double, TemperatureUnits> SelectedTemperatureUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,TemperatureUnits>> TemperatureUnits => 
            _localization.Temperature.AvailableUnits;
        
        public string TemperatureIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Temperature);
    }
}
