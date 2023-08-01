using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Mavlink.Vehicle;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Core;

public class CoordinatesCalculatorViewModelConfig
{
    public int SelectedFromAltUnitIndex { get; set; }
    public int SelectedToAltUnitIndex { get; set; }
    public int SelectedFromLatLongUnitIndex { get; set; }
    public int SelectedToLatLongUnitIndex { get; set; }
    public int SelectedFromStandardIndex { get; set; }
    public int SelectedToStandardIndex { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string Altitude { get; set; }
}

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CoordinatesCalculatorViewModel : ViewModelBaseWithValidation
{
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;
    
    public const string UriString = ShellViewModel.UriString + "/tools/coordinates-calculator";
    public static readonly Uri Uri = new(UriString);
    private readonly CoordinatesCalculatorViewModelConfig _config;

    [ImportingConstructor]
    public CoordinatesCalculatorViewModel(IConfiguration cfg, ILocalizationService loc) : this()
    {
        _cfg = cfg;
        _loc = loc;
        _config = cfg.Get<CoordinatesCalculatorViewModelConfig>();
        
        SelectedFromLatLongUnitIndex = _config.SelectedFromLatLongUnitIndex;
        SelectedToLatLongUnitIndex = _config.SelectedToLatLongUnitIndex;
        SelectedFromStandardIndex = _config.SelectedFromStandardIndex;
        SelectedToStandardIndex = _config.SelectedToStandardIndex;
        SelectedFromAltUnitIndex = _config.SelectedFromAltUnitIndex;
        SelectedToAltUnitIndex = _config.SelectedToAltUnitIndex;
        FromLatitude = _config.Latitude;
        FromLongitude = _config.Longitude;
        FromAltitude = _config.Altitude;
        
        this.WhenPropertyChanged(_ => _.SelectedFromLatLongUnitIndex)
            .Subscribe(_ =>
            {
                SelectedFromLatLongUnit = LatLongUnits[_.Value].Unit;
                _config.SelectedFromLatLongUnitIndex = _.Value;
                cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.SelectedToLatLongUnitIndex)
            .Subscribe(_ =>
            {
                SelectedToLatLongUnit = LatLongUnits[_.Value].Unit;
                _config.SelectedToLatLongUnitIndex = _.Value;
                cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.SelectedFromStandardIndex)
            .Subscribe(_ =>
            {
                _config.SelectedFromStandardIndex = _.Value;
                cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.SelectedToStandardIndex)
            .Subscribe(_ =>
            {
                _config.SelectedToStandardIndex = _.Value;
                cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.SelectedFromAltUnitIndex)
            .Subscribe(_ =>
            {
                SelectedFromAltUnit = AltUnits[_.Value].Unit;
                _config.SelectedFromAltUnitIndex = _.Value;
                cfg.Set(_config);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.SelectedToAltUnitIndex)
            .Subscribe(_ =>
            {
                SelectedToAltUnit = AltUnits[_.Value].Unit;
                _config.SelectedToAltUnitIndex = _.Value;
                cfg.Set(_config);
            })
            .DisposeItWith(Disposable);

        this.WhenPropertyChanged(_ => _.FromLatitude)
            .Subscribe(_ =>
            {
                UpdateLatitude(_.Value);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.FromLongitude)
            .Subscribe(_ =>
            {
                UpdateLongitude(_.Value);
            })
            .DisposeItWith(Disposable);
        
        this.WhenPropertyChanged(_ => _.FromAltitude)
            .Subscribe(_ =>
            {
                UpdateAltitude(_.Value);
            })
            .DisposeItWith(Disposable);

        this.WhenAnyPropertyChanged(nameof(SelectedFromLatLongUnitIndex), 
                nameof(SelectedToLatLongUnitIndex), 
                nameof(SelectedFromStandardIndex), 
                nameof(SelectedToStandardIndex), 
                nameof(SelectedFromAltUnitIndex), 
                nameof(SelectedToAltUnitIndex))
            .Subscribe(_ =>
            {
                UpdateLatitude(FromLatitude);
                UpdateLongitude(FromLongitude);
                UpdateAltitude(FromAltitude);
            })
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.FromLatitude,
                _ => _loc.Latitude.IsValid(_),
                _ => _loc.Latitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.FromLongitude,
                _ => _loc.Longitude.IsValid(_),
                _ => _loc.Longitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.FromAltitude,
                _ => _loc.Altitude.IsValid(_),
                _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        SwapLatLongUnits = ReactiveCommand.Create(SwapLatLongUnitsImpl).DisposeItWith(Disposable);
        SwapStandards = ReactiveCommand.Create(SwapStandardsImpl).DisposeItWith(Disposable);
        SwapAltUnits = ReactiveCommand.Create(SwapAltUnitsImpl).DisposeItWith(Disposable);
    }

    public CoordinatesCalculatorViewModel() : base(Uri)
    {
        
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
    }

    private void SwapStandardsImpl()
    {
        (SelectedToStandardIndex, SelectedFromStandardIndex) = 
            (SelectedFromStandardIndex, SelectedToStandardIndex);
    }
    
    private void SwapLatLongUnitsImpl()
    {
        (SelectedToLatLongUnitIndex, SelectedFromLatLongUnitIndex) = 
            (SelectedFromLatLongUnitIndex, SelectedToLatLongUnitIndex);
    }
    
    private void SwapAltUnitsImpl()
    {
        (SelectedToAltUnitIndex, SelectedFromAltUnitIndex) = 
            (SelectedFromAltUnitIndex, SelectedToAltUnitIndex);
    }

    private void UpdateLatitude(string latitude)
    {
        if (!string.IsNullOrWhiteSpace(latitude) && 
            GeoPointLatitude.TryParse(latitude, out var latResult))
        {
            var currentSiValue =
                _loc.Latitude.AvailableUnits
                    .Single(u => u.Unit == SelectedFromLatLongUnit)
                    .ConvertToSi(latResult);
                    
            var point = GetCorrectedGeoPoint(new GeoPoint(currentSiValue, 0, 0));
                    
            ToLatitude = _loc.Latitude.AvailableUnits
                .Single(u => u.Unit == SelectedToLatLongUnit)
                .FromSiToString(point.Latitude);
                    
            _config.Latitude = latitude;
            _cfg.Set(_config);
        }
    }
    
    private void UpdateLongitude(string longitude)
    {
        if (!string.IsNullOrWhiteSpace(longitude) && 
            GeoPointLongitude.TryParse(longitude, out var longResult))
        {
            var currentSiValue =
                _loc.Longitude.AvailableUnits
                    .Single(u => u.Unit == SelectedFromLatLongUnit)
                    .ConvertToSi(longResult);
                    
            var point = GetCorrectedGeoPoint(new GeoPoint(0, currentSiValue, 0));
                    
            ToLongitude = _loc.Longitude.AvailableUnits
                .Single(u => u.Unit == SelectedToLatLongUnit)
                .FromSiToString(point.Longitude);
                    
            _config.Longitude = longitude;
            _cfg.Set(_config);
        }
    }
    
    private void UpdateAltitude(string altitude)
    {
        if (!string.IsNullOrWhiteSpace(altitude) && 
            double.TryParse(altitude, out var altResult))
        {
            var currentSiValue =
                _loc.Altitude.AvailableUnits
                    .Single(u => u.Unit == SelectedFromAltUnit)
                    .ConvertToSi(altResult);

            var point = GetCorrectedGeoPoint(new GeoPoint(0, 0, currentSiValue));
                    
            ToAltitude = _loc.Altitude.AvailableUnits
                .Single(u => u.Unit == SelectedToAltUnit)
                .FromSiToString(point.Altitude);
                    
            _config.Altitude = altitude;
            _cfg.Set(_config);
        }
    }

    private GeoPoint GetCorrectedGeoPoint(GeoPoint point)
    {
        // WGS 84 -> PZ 90
        if (SelectedFromStandardIndex == 0 & SelectedToStandardIndex == 1) return point.WGS84_PZ90();
        // PZ 90 -> WGS 84
        if (SelectedFromStandardIndex == 1 & SelectedToStandardIndex == 0) return point.PZ90_WGS84();
        return point;
    }
    
    [Reactive]
    public ICommand SwapStandards { get; set; }
    
    [Reactive]
    public ICommand SwapLatLongUnits { get; set; }
    
    [Reactive]
    public ICommand SwapAltUnits { get; set; }
    
    [Reactive]
    public int SelectedFromStandardIndex { get; set; }
    
    [Reactive]
    public int SelectedToStandardIndex { get; set; }
    
    [Reactive]
    public int SelectedFromLatLongUnitIndex { get; set; }
    
    [Reactive]
    public int SelectedToLatLongUnitIndex { get; set; }
    
    [Reactive]
    public string SelectedFromLatLongUnit { get; set; }
    
    [Reactive]
    public string SelectedToLatLongUnit { get; set; }
    
    [Reactive]
    public int SelectedFromAltUnitIndex { get; set; }
    
    [Reactive]
    public int SelectedToAltUnitIndex { get; set; }
    
    [Reactive]
    public string SelectedFromAltUnit { get; set; }
    
    [Reactive]
    public string SelectedToAltUnit { get; set; }
    
    [Reactive]
    public string FromLatitude { get; set; }
    
    [Reactive]
    public string ToLatitude { get; set; }
    
    [Reactive]
    public string FromLongitude { get; set; }
    
    [Reactive]
    public string ToLongitude { get; set; }
    
    [Reactive]
    public string FromAltitude { get; set; }
    
    [Reactive]
    public string ToAltitude { get; set; }
    
    public IMeasureUnitItem<double, DegreeUnits>[] LatLongUnits => _loc.Degree.AvailableUnits.ToArray();

    public IMeasureUnitItem<double, AltitudeUnits>[] AltUnits => _loc.Altitude.AvailableUnits.ToArray();
    
    public string[] Standards => new[]
    {
        "WSG 84",
        "ПЗ 90"
    };
}