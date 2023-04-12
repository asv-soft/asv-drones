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
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double Altitude { get; set; }
    public double Accuracy { get; set; }
}

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class FixedModeViewModel : ViewModelBaseWithValidation
{
    private readonly IGbsDevice _gbsDevice;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    
    private const double MinimumAccuracyDistance = 0.01;
    private const int MinimumLatitudeValue = -90;
    private const int MaximumLatitudeValue = 90;
    private const int MinimumLongitudeValue = -180;
    private const int MaximumLongitudeValue = 180;
    
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

        var fixedModeConfig = _configuration.Get<FixedModeConfig>();

        Latitude = _loc.Latitude.FromSiToString(fixedModeConfig.Latitude);
        Longitude = _loc.Longitude.FromSiToString(fixedModeConfig.Longitude);
        Altitude = _loc.Altitude.FromSiToString(fixedModeConfig.Altitude);
        Accuracy = _loc.Distance.FromSiToString(fixedModeConfig.Accuracy);

#region Validation Rules

        this.ValidationRule(x => x.Accuracy,
                _ => _loc.Distance.IsValid(MinimumAccuracyDistance, double.MaxValue, _),
                string.Format(RS.AutoModeViewModel_Accuracy_ValidValue,
                    _loc.Distance.FromSiToString(MinimumAccuracyDistance)))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Latitude, _ => _loc.Latitude.IsValid(MinimumLatitudeValue, MaximumLatitudeValue, _),
                _ => _loc.Latitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
        
        this.ValidationRule(x => x.Longitude, _ => _loc.Longitude.IsValid(MinimumLongitudeValue, MaximumLongitudeValue, _),
                _ => _loc.Longitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
      

        this.ValidationRule(x => x.Altitude, _ => _loc.Altitude.IsValid(_),
                _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        
#endregion
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        dialog.PrimaryButtonCommand = ReactiveCommand
            .CreateFromTask(SetUpFixedMode, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
            .DisposeItWith(Disposable);
    }

    private async Task SetUpFixedMode(CancellationToken cancel)
    {
        if (_gbsDevice == null) return;
        var lat = _loc.Latitude.ConvertToSi(Latitude);
        var lon = _loc.Longitude.ConvertToSi(Longitude);
        var alt = _loc.Altitude.ConvertToSi(Altitude);
        var acc = _loc.Distance.ConvertToSi(Accuracy);
        _configuration.Set(new FixedModeConfig()
        {
            Longitude = lon,
            Latitude = lat,
            Altitude = alt,
            Accuracy = acc
        });
        try
        {
            await _gbsDevice.DeviceClient.StartFixedMode(
                new GeoPoint(lat,lon, alt),
                (float)_loc.Distance.ConvertToSi(Accuracy),
                cancel);
        }
        catch (Exception e)
        {
            _logService.Error("", string.Format(RS.FixedModeViewModel_StartFailed, e.Message), e);
        }
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
    public string LatitudeUnits => _loc.Latitude.CurrentUnit.Value.Unit;
    public string LongitudeUnits => _loc.Longitude.CurrentUnit.Value.Unit;
    public string AltitudeUnits => _loc.Altitude.CurrentUnit.Value.Unit;
}