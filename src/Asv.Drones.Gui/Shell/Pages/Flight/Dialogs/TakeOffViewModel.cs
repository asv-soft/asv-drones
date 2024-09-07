using System;
using System.Composition;
using System.Globalization;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui;

public class TakeOffViewModelConfig
{
    public double TakeOffAltitudeAglMeter { get; set; } = 30;
}

public class TakeOffViewModel : DisposableReactiveObjectWithValidation
{
    private readonly IConfiguration _cfg;
    private readonly ILocalizationService _loc;

    private readonly TakeOffViewModelConfig _config;
    public const double MinimumAltitudeMeter = 1;
    private bool _isUpdating;

    [ImportingConstructor]
    public TakeOffViewModel(IConfiguration cfg, ILocalizationService loc, IVehicleClient vehicle)
    {
        _cfg = cfg;
        _loc = loc;
        _config = cfg.Get<TakeOffViewModelConfig>();
        AltitudeAgl = _loc.Altitude.FromSiToString(_config.TakeOffAltitudeAglMeter);
        AltitudeGnss = _loc.Altitude.FromSiToString(vehicle.Position.Current.Value.Altitude);
        CurrentAgl = _loc.Altitude.FromSiToStringWithUnits(vehicle.Position.AltitudeAboveHome.Value);

        vehicle.Position.Current.Subscribe(x =>
                CurrentAgl = string.Format(RS.TakeOffAnchorActionViewModel_CurrentAgl_Format, 
                        _loc.Altitude.FromSiToStringWithUnits(vehicle.Position.AltitudeAboveHome.Value)))
            .DisposeItWith(Disposable);

        this.WhenValueChanged(x => x.AltitudeAgl)
            .Subscribe(x =>
            {
                if (_isUpdating) return;

                _isUpdating = true;
                if (double.TryParse(x, NumberStyles.Number, CultureInfo.InvariantCulture, out var agl))
                {
                    var newGnss = vehicle.Position.Current.Value.Altitude + agl;
                    AltitudeGnss = newGnss.ToString(CultureInfo.InvariantCulture);
                }
                _isUpdating = false;
            }).DisposeItWith(Disposable);
        
        this.WhenValueChanged(x => x.AltitudeGnss)
            .Subscribe(x =>
            {
                if (_isUpdating) return;

                _isUpdating = true;
                if (double.TryParse(x, NumberStyles.Number, CultureInfo.InvariantCulture, out var gnss))
                {
                    var newAgl = gnss - vehicle.Position.Current.Value.Altitude;
                    AltitudeAgl = newAgl.ToString(CultureInfo.InvariantCulture);
                }
                _isUpdating = false;
            }).DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.AltitudeAgl, 
            _ => _loc.Altitude.IsValid(_), 
            _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.AltitudeAgl,
                _ => _loc.Altitude.IsValid(_) && _loc.Altitude.ConvertToSi(_) >= MinimumAltitudeMeter,
                string.Format(RS.TakeOffAnchorActionViewModel_ValidValue,
                    _loc.Altitude.FromSiToString(MinimumAltitudeMeter)))
            .DisposeItWith(Disposable);
        
        this.ValidationRule(x => x.AltitudeGnss, 
                _ => _loc.Altitude.IsValid(_), 
                _ => _loc.Altitude.GetErrorMessage(_))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.AltitudeGnss,
                _ => _loc.Altitude.IsValid(_) && _loc.Altitude.ConvertToSi(_) >= MinimumAltitudeMeter,
                string.Format(RS.TakeOffAnchorActionViewModel_ValidValue,
                    _loc.Altitude.FromSiToString(MinimumAltitudeMeter)))
            .DisposeItWith(Disposable);
    }

    public TakeOffViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        dialog.PrimaryButtonCommand =
            ReactiveCommand.Create(() =>
                {
                    _config.TakeOffAltitudeAglMeter = _loc.Altitude.ConvertToSi(AltitudeAgl);
                    _cfg.Set(_config);
                },
                this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
                .DisposeItWith(Disposable);
    }

    [Reactive] public string AltitudeAgl { get; set; }
    [Reactive] public string AltitudeGnss { get; set; }
    [Reactive] public string CurrentAgl { get; set; }

    public string Units => _loc.Altitude.CurrentUnit.Value.Unit;
}