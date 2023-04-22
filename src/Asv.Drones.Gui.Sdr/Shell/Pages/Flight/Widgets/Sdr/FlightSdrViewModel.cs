using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Asv.Mavlink.V2.Common;
using DynamicData;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat.ModeDetection;

namespace Asv.Drones.Gui.Sdr;

public class FlightSdrViewModel:FlightSdrWidgetBase
{
    private readonly ReadOnlyObservableCollection<ISdrRttItem> _rttItems;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    public static Uri GenerateUri(ISdrClientDevice sdr) => FlightSdrWidgetBase.GenerateUri(sdr,"sdr");

    public FlightSdrViewModel()
    {
        
    }
    
    public FlightSdrViewModel(ISdrClientDevice payload, ILogService log, ILocalizationService loc, IConfiguration configuration, IEnumerable<ISdrRttItemProvider> rttItems)
        :base(payload, GenerateUri(payload))
    {
        _logService = log;
        _loc = loc;
        _configuration = configuration;
        
        Icon = MaterialIconKind.Memory;
        Title = "Payload";
        Location = WidgetLocation.Right;

        payload.Sdr.SupportedModes.DistinctUntilChanged()
            .Subscribe(UpdateModes)
            .DisposeItWith(Disposable);
        payload.Sdr.CustomMode.DistinctUntilChanged()
            .Subscribe(UpdateSelectedMode)
            .DisposeItWith(Disposable);
        
        rttItems
            .SelectMany(_ => _.Create(payload))
            .OrderBy(_=>_.Order)
            .AsObservableChangeSet()
            .AutoRefresh(_=>_.IsVisible)
            .Filter(_=>_.IsVisible)
            .Bind(out _rttItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        UpdateMode = ReactiveCommand.CreateFromTask(cancel=>Payload.Sdr.SetMode(SelectedMode.Mode,Frequency,1,1,cancel));
        UpdateMode.ThrownExceptions.Subscribe(OnCommandError).DisposeItWith(Disposable);
        
    }

    private void UpdateSelectedMode(AsvSdrCustomMode mode)
    {
        SelectedMode = Modes.FirstOrDefault(__ => __.Mode == mode);
    }

    private void OnCommandError(Exception ex)
    {
        _logService.Error("Set mode",$"Error to set payload mode",ex); // TODO: Localize
    }

    private void UpdateModes(AsvSdrCustomModeFlag flag)
    {
        Modes.Clear();
        Modes.Add(new SdrModeViewModel("IDLE", AsvSdrCustomMode.AsvSdrCustomModeIdle, MaterialIconKind.Numeric0CircleOutline));
        
        if (flag.HasFlag(AsvSdrCustomModeFlag.AsvSdrCustomModeFlagGp))
        {
            Modes.Add(new SdrModeViewModel("GP", AsvSdrCustomMode.AsvSdrCustomModeGp, MaterialIconKind.AlphaGCircle));
        }
        if (flag.HasFlag(AsvSdrCustomModeFlag.AsvSdrCustomModeFlagLlz))
        {
            Modes.Add(new SdrModeViewModel("LLZ", AsvSdrCustomMode.AsvSdrCustomModeLlz, MaterialIconKind.AlphaLCircle));
        }
        if (flag.HasFlag(AsvSdrCustomModeFlag.AsvSdrCustomModeFlagVor))
        {
            Modes.Add(new SdrModeViewModel("VOR", AsvSdrCustomMode.AsvSdrCustomModeVor,MaterialIconKind.AlphaVCircle));
        }
        UpdateSelectedMode(Payload.Sdr.CustomMode.Value);
    }

    public ReadOnlyObservableCollection<ISdrRttItem> RttItems => _rttItems;
    public ObservableCollection<SdrModeViewModel> Modes { get; } = new();
    [Reactive]
    public SdrModeViewModel? SelectedMode { get; set; }
    [Reactive]
    public ulong Frequency { get; set; }

    public ReactiveCommand<Unit,MavResult> UpdateMode { get; }
}

public class SdrModeViewModel:ReactiveObject
{
    public MaterialIconKind Icon { get; }
    public string Name { get; }
    public AsvSdrCustomMode Mode { get; }

    public SdrModeViewModel(string name, AsvSdrCustomMode mode,MaterialIconKind icon)
    {
        Name = name;
        Mode = mode;
        Icon = icon;
    }
}