using System.ComponentModel.Composition;
using Asv.Common;
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

            _localization.LatitudeAndLongitude.CurrentUnit.Subscribe(_ => SelectedLatitudeLongitudeUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedLatitudeLongitudeUnit)
                .Subscribe( _localization.LatitudeAndLongitude.CurrentUnit)
                .DisposeItWith(Disposable);
            
            _localization.Velocity.CurrentUnit.Subscribe(_ => SelectedVelocityUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedVelocityUnit)
                .Subscribe(_localization.Velocity.CurrentUnit)
                .DisposeItWith(Disposable);
        }
        
        [Reactive]
        public IMeasureUnitItem<double,AltitudeUnits> SelectedAltitudeUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,AltitudeUnits>> AltitudeUnits => _localization.Altitude.AvailableUnits;

        [Reactive]
        public IMeasureUnitItem<double,DistanceUnits> SelectedDistanceUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,DistanceUnits>> DistanceUnits => _localization.Distance.AvailableUnits;

        [Reactive]
        public IMeasureUnitItem<double,LatitudeLongitudeUnits> SelectedLatitudeLongitudeUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,LatitudeLongitudeUnits>> LatitudeLongitudeUnits => _localization.LatitudeAndLongitude.AvailableUnits;
        
        [Reactive]
        public IMeasureUnitItem<double,VelocityUnits> SelectedVelocityUnit { get; set; }

        public IEnumerable<IMeasureUnitItem<double,VelocityUnits>> VelocityUnits => _localization.Velocity.AvailableUnits;
    }
}