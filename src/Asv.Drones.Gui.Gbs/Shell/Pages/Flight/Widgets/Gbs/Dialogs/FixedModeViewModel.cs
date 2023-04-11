using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Gbs;

public class FixedModeConfig
{
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public string Altitude { get; set; }
    public string Accuracy { get; set; }
}

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class FixedModeViewModel : ViewModelBaseWithValidation
{
    private readonly IGbsDevice _gbsDevice;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    private readonly CancellationToken _ctx;
    
    private const double MinimumAccuracyDistance = 1;
    private const double MinimumLatitudeOrLongitude = 1;
    private const double MinimumAltitude = 1;
    
    public FixedModeViewModel() : base(new Uri(FlightGbsViewModel.Uri, "fixed"))
    {
    }
    
    [ImportingConstructor]
    public FixedModeViewModel(IGbsDevice gbsDevice, ILogService logService, ILocalizationService loc, IConfiguration configuration, CancellationToken ctx) : this()
    {
        _gbsDevice = gbsDevice;
        _logService = logService;
        _configuration = configuration;
        _loc = loc;
        _ctx = ctx;

        if (_configuration.Exist<FixedModeConfig>(nameof(FixedModeViewModel)))
        {
            var fixedModeConfig = _configuration.Get<FixedModeConfig>(nameof(FixedModeViewModel));

            Latitude = fixedModeConfig.Latitude;
            Longitude = fixedModeConfig.Longitude;
            Altitude = fixedModeConfig.Altitude;
            Accuracy = fixedModeConfig.Accuracy;
        }

#region Validation Rules

        this.ValidationRule(x => x.Accuracy, _ => _loc.Distance.IsValid(_), _ => _loc.Distance.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        this.ValidationRule(x => x.Accuracy,
                _ => _loc.Distance.IsValid(_) && _loc.Distance.ConvertToSI(_) >= MinimumAccuracyDistance,
                string.Format(RS.FixedModeViewModel_Accuracy_ValidValue,
                    _loc.Distance.FromSIToString(MinimumAccuracyDistance)))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Latitude, _ => _loc.LatitudeAndLongitude.IsValid(_),
                _ => _loc.LatitudeAndLongitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        this.ValidationRule(x => x.Latitude,
                _ => _loc.LatitudeAndLongitude.IsValid(_) &&
                     _loc.LatitudeAndLongitude.ConvertToSI(_) >= MinimumLatitudeOrLongitude,
                string.Format(RS.FixedModeViewModel_Latitude_ValidValue,
                    _loc.LatitudeAndLongitude.FromSIToString(MinimumLatitudeOrLongitude)))
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.Longitude, _ => _loc.LatitudeAndLongitude.IsValid(_),
                _ => _loc.LatitudeAndLongitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        this.ValidationRule(x => x.Longitude,
                _ => _loc.LatitudeAndLongitude.IsValid(_) &&
                     _loc.LatitudeAndLongitude.ConvertToSI(_) >= MinimumLatitudeOrLongitude,
                string.Format(RS.FixedModeViewModel_Longitude_ValidValue,
                    _loc.LatitudeAndLongitude.FromSIToString(MinimumLatitudeOrLongitude)))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Altitude, _ => _loc.Altitude.IsValid(_),
                _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        this.ValidationRule(x => x.Altitude,
                _ => _loc.Altitude.IsValid(_) &&
                     _loc.Altitude.ConvertToSI(_) >= MinimumAltitude,
                string.Format(RS.FixedModeViewModel_Altitude_ValidValue,
                    _loc.Altitude.FromSIToString(MinimumAltitude)))
            .DisposeItWith(Disposable);
        
#endregion
        
        this.WhenAnyValue(_ => _._gbsDevice.MavlinkClient.Gbs.Status.Value.Lat)
            .Subscribe(_ => Latitude = _loc.LatitudeAndLongitude.FromSIToString(_))
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _._gbsDevice.MavlinkClient.Gbs.Status.Value.Lng)
            .Subscribe(_ => Longitude = _loc.LatitudeAndLongitude.FromSIToString(_))
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _._gbsDevice.MavlinkClient.Gbs.Status.Value.Alt)
            .Subscribe(_ => Altitude = _loc.Altitude.FromSIToString(_))
            .DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _._gbsDevice.MavlinkClient.Gbs.Status.Value.Accuracy)
            .Subscribe(_ => Accuracy = _loc.Distance.FromSIToString(_))
            .DisposeItWith(Disposable);
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        dialog.PrimaryButtonCommand = ReactiveCommand
            .Create(SetUpFixedMode, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
            .DisposeItWith(Disposable);
    }

    private void SetUpFixedMode()
    {
        if (_gbsDevice == null) return;

        try
        {
            _gbsDevice.DeviceClient.StartFixedMode(
                new GeoPoint(_loc.LatitudeAndLongitude.ConvertToSI(Latitude), 
                    _loc.LatitudeAndLongitude.ConvertToSI(Longitude), 
                    _loc.Altitude.ConvertToSI(Altitude)),
                (float)_loc.Distance.ConvertToSI(Accuracy),
                _ctx);
        }
        catch (Exception e)
        {
            _logService.Error("", string.Format(RS.FixedModeViewModel_StartFailed, e.Message), e);
        }

        var fixedModeConfig = new FixedModeConfig()
        {
            Longitude = Longitude,
            Latitude = Latitude,
            Altitude = Altitude,
            Accuracy = Accuracy
        };
        
        _configuration.Set(nameof(FixedModeViewModel), fixedModeConfig);
    }
    
    [Reactive]
    public string Latitude { get; set; } = "0";
    [Reactive]
    public string Longitude { get; set; } = "0";
    [Reactive]
    public string Altitude { get; set; } = "0";
    [Reactive]
    public string Accuracy { get; set; } = "0";

    public string AccuracyUnits => _loc.Distance.CurrentUnit.Value.Unit;
    public string LatitudeAndLongitudeUnits => _loc.LatitudeAndLongitude.CurrentUnit.Value.Unit;
    public string AltitudeUnits => _loc.Altitude.CurrentUnit.Value.Unit;
}