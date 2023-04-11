using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Gbs;

public class AutoModeConfig
{
    public double Observation { get; set; }
    public double Accuracy { get; set; }
}

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class AutoModeViewModel : ViewModelBaseWithValidation
{
    private readonly IGbsDevice _gbsDevice;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;

    private const double MinimumAccuracyDistance = 0.01;
    private const double MinimumObservationTime = 1;
    
    public AutoModeViewModel() : base(new Uri(FlightGbsViewModel.Uri, "auto"))
    {
    }
    
    [ImportingConstructor]
    public AutoModeViewModel(IGbsDevice gbsDevice, ILogService logService, ILocalizationService loc, IConfiguration configuration, CancellationToken ctx) : this()
    {
        _gbsDevice = gbsDevice;
        _logService = logService;
        _configuration = configuration;
        _loc = loc;
        
        var autoModeConfig = _configuration.Get<AutoModeConfig>();
        Accuracy = _loc.Distance.FromSiToString(autoModeConfig.Accuracy);
        Observation = autoModeConfig.Observation.ToString();
        
#region Validation Rules

        this.ValidationRule(x => x.Observation, _ => Convert.ToDouble(_) > 0,
                string.Format(RS.AutoModeViewModel_Observation_ValidValue, MinimumObservationTime))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Accuracy,
                _ => _loc.Distance.IsValid(MinimumAccuracyDistance, double.MaxValue, _) && _loc.Distance.ConvertToSi(_) >= MinimumAccuracyDistance,
                string.Format(RS.AutoModeViewModel_Accuracy_ValidValue,
                    _loc.Distance.FromSiToString(MinimumAccuracyDistance)))
            .DisposeItWith(Disposable);

#endregion
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        dialog.PrimaryButtonCommand = ReactiveCommand
            .CreateFromTask(SetUpAutoMode, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
            .DisposeItWith(Disposable);
    }

    private async Task SetUpAutoMode(CancellationToken cancel)
    {
        if (_gbsDevice == null) return;
        var acc = _loc.Distance.ConvertToSi(Accuracy);
        var obs = Convert.ToDouble(Observation);
        _configuration.Set(new AutoModeConfig
        {
            Accuracy = acc,
            Observation = obs
        });
        try
        {
            await _gbsDevice.DeviceClient.StartAutoMode((float)obs, (float)acc, cancel);
        }
        catch (Exception e)
        {
            _logService.Error("", string.Format(RS.AutoModeViewModel_StartFailed, e.Message), e);
        }
        
        
    }

    [Reactive] public string Observation { get; set; } = "0";
    [Reactive] public string Accuracy { get; set; } = "0";

    public string AccuracyUnits => _loc.Distance.CurrentUnit.Value.Unit;
}