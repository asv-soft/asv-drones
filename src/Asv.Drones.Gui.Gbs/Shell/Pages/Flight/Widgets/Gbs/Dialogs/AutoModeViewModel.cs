using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Gbs;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class AutoModeViewModel : ViewModelBaseWithValidation
{
    private readonly IGbsDevice _gbsDevice;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly CancellationToken _ctx;

    private const double MinimumAccuracyDistance = 1;
    private const double MinimumObservationTime = 1;
    
    public AutoModeViewModel() : base(new Uri(FlightGbsViewModel.Uri, "auto"))
    {
    }
    
    [ImportingConstructor]
    public AutoModeViewModel(IGbsDevice gbsDevice, ILogService logService, ILocalizationService loc, CancellationToken ctx) : this()
    {
        _gbsDevice = gbsDevice;
        _logService = logService;
        _loc = loc;
        _ctx = ctx;
        
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

        this.WhenAnyValue(_ => _._gbsDevice.MavlinkClient.Gbs.Status.Value.Observation)
            .Subscribe(_ => Observation = _)
            .DisposeItWith(Disposable);

        this.WhenAnyValue(_ => _._gbsDevice.MavlinkClient.Gbs.Status.Value.Accuracy)
            .Subscribe(_ => Accuracy = _loc.Distance.FromSIToString(_))
            .DisposeItWith(Disposable);
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
    }

    [Reactive] public ushort Observation { get; set; }
    [Reactive] public string Accuracy { get; set; } = "0";

    public string AccuracyUnits => _loc.Distance.CurrentUnit.Value.Unit;
}