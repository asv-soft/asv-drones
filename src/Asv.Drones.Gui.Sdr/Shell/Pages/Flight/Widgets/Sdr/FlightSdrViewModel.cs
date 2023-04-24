using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
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
        if (Design.IsDesignMode)
        {
            Icon = MaterialIconKind.Memory;
            _rttItems = new ReadOnlyObservableCollection<ISdrRttItem>(new ObservableCollection<ISdrRttItem>(new List<ISdrRttItem>
            {
                new SdrRttItemLlzViewModel(),
                new SdrRttItemLlzViewModel(),
                new SdrRttItemLlzViewModel(),
            }));
        }
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
        payload.Sdr.IsRecordStarted.DistinctUntilChanged()
            .Subscribe(_=>IsRecordStarted = _)
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
        
        UpdateMode = ReactiveCommand.CreateFromTask(cancel=>Payload.Sdr.SetModeAndCheckResult(SelectedMode.Mode,Frequency,1,1,cancel));
        UpdateMode.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error("Set mode",$"Error to set payload mode",ex); // TODO: Localize
        }).DisposeItWith(Disposable);
        
        StartRecord = ReactiveCommand.CreateFromTask(async cancel=>
        {
            await Payload.Sdr.StartRecordAndCheckResult("Record1", cancel);
            await Payload.Sdr.CurrentRecordSetTagAndCheckResult("tag1", 10, cancel);
        },this.WhenAnyValue(_=>_.IsRecordStarted).Select(_=>!_));
        StartRecord.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error("Rec start",$"Error to set payload mode",ex);
        }).DisposeItWith(Disposable);
        
        StopRecord = ReactiveCommand.CreateFromTask(cancel=>Payload.Sdr.StopRecordAndCheckResult(cancel),this.WhenAnyValue(_=>_.IsRecordStarted));
        StopRecord.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error("Rec stop",$"Error to set payload mode",ex);
        }).DisposeItWith(Disposable);
        
    }

    private void UpdateSelectedMode(AsvSdrCustomMode mode)
    {
        SelectedMode = Modes.FirstOrDefault(__ => __.Mode == mode);
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
    [Reactive]
    public bool IsRecordStarted { get; set; }
    public ReactiveCommand<Unit,Unit> UpdateMode { get; }
    public ReactiveCommand<Unit,Unit> StartRecord { get; }
    public ReactiveCommand<Unit,Unit> StopRecord { get; }
    
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