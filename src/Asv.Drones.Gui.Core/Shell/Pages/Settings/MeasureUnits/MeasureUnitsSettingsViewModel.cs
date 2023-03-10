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
            
            _localization.CurrentAltitudeUnit.Subscribe(_ => SelectedAltitudeUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedAltitudeUnit)
                .Subscribe(_localization.CurrentAltitudeUnit)
                .DisposeItWith(Disposable);
            
            _localization.CurrentDistanceUnit.Subscribe(_ => SelectedDistanceUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedDistanceUnit)
                .Subscribe(_localization.CurrentDistanceUnit)
                .DisposeItWith(Disposable);

            _localization.CurrentLatitudeLongitudeUnit.Subscribe(_ => SelectedLatitudeLongitudeUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedLatitudeLongitudeUnit)
                .Subscribe(_localization.CurrentLatitudeLongitudeUnit)
                .DisposeItWith(Disposable);
            
            _localization.CurrentVelocityUnit.Subscribe(_ => SelectedVelocityUnit = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedVelocityUnit)
                .Subscribe(_localization.CurrentVelocityUnit)
                .DisposeItWith(Disposable);
        }
        
        [Reactive]
        public AltitudeUnitItem SelectedAltitudeUnit { get; set; }

        public IEnumerable<AltitudeUnitItem> AltitudeUnits => _localization.AltitudeUnits;

        [Reactive]
        public DistanceUnitItem SelectedDistanceUnit { get; set; }

        public IEnumerable<DistanceUnitItem> DistanceUnits => _localization.DistanceUnits;

        [Reactive]
        public LatitudeLongitudeUnitItem SelectedLatitudeLongitudeUnit { get; set; }

        public IEnumerable<LatitudeLongitudeUnitItem> LatitudeLongitudeUnits => _localization.LatitudeLongitudeUnits;
        
        [Reactive]
        public VelocityUnitItem SelectedVelocityUnit { get; set; }

        public IEnumerable<VelocityUnitItem> VelocityUnits => _localization.VelocityUnits;
    }
}