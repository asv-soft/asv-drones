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

public class AutoModeConfig
{
    public ushort Observation { get; set; }
    public string Accuracy { get; set; }
}

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class AutoModeViewModel : ViewModelBaseWithValidation
{
    private readonly IGbsDevice _gbsDevice;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    private readonly CancellationToken _ctx;

    private const double MinimumAccuracyDistance = 1;
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
        _ctx = ctx;
        
        if (_configuration.Exist<AutoModeConfig>(nameof(AutoModeViewModel)))
        {
            var autoModeConfig = _configuration.Get<AutoModeConfig>(nameof(AutoModeViewModel));

            Accuracy = autoModeConfig.Accuracy;
            Observation = autoModeConfig.Observation;
        }
        
#region Validation Rules

        this.ValidationRule(x => x.Observation, _ => _ > 0,
                string.Format(RS.AutoModeViewModel_Observation_ValidValue, MinimumObservationTime))
            .DisposeItWith(Disposable);

        this.ValidationRule(x => x.Accuracy, _ => _loc.Distance.IsValid(_), _ => _loc.Distance.GetErrorMessage(_))
            .DisposeItWith(Disposable);
        this.ValidationRule(x => x.Accuracy,
                _ => _loc.Distance.IsValid(_) && _loc.Distance.ConvertToSI(_) >= MinimumAccuracyDistance,
                string.Format(RS.AutoModeViewModel_Accuracy_ValidValue,
                    _loc.Distance.FromSIToString(MinimumAccuracyDistance)))
            .DisposeItWith(Disposable);

#endregion
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));

        dialog.PrimaryButtonCommand = ReactiveCommand
            .Create(SetUpAutoMode, this.IsValid().Do(_ => dialog.IsPrimaryButtonEnabled = _))
            .DisposeItWith(Disposable);
    }

    private void SetUpAutoMode()
    {
        if (_gbsDevice == null) return;

        try
        {
            _gbsDevice.DeviceClient.StartAutoMode(Observation, (float)_loc.Distance.ConvertToSI(Accuracy), _ctx);
        }
        catch (Exception e)
        {
            _logService.Error("", string.Format(RS.AutoModeViewModel_StartFailed, e.Message), e);
        }
        
        var autoModeConfig = new AutoModeConfig()
        {
            Accuracy = Accuracy,
            Observation = Observation
        };
        
        _configuration.Set(nameof(AutoModeViewModel), autoModeConfig);
    }

    [Reactive] public ushort Observation { get; set; }
    [Reactive] public string Accuracy { get; set; } = "0";

    public string AccuracyUnits => _loc.Distance.CurrentUnit.Value.Unit;
}