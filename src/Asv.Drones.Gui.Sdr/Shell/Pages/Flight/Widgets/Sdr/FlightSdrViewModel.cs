using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class FlightSdrViewModel:FlightSdrWidgetBase
{
    private readonly ObservableCollection<ISdrRttItem> _rttItems = new();
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    private readonly ISdrRttItemProvider[] _providers;
    public static Uri GenerateUri(ISdrClientDevice sdr) => FlightSdrWidgetBase.GenerateUri(sdr,"sdr");

    public FlightSdrViewModel()
    {
        if (Design.IsDesignMode)
        {
            Icon = MaterialIconKind.Memory;
            _rttItems = new ObservableCollection<ISdrRttItem>(new List<ISdrRttItem>
            {
                new SdrRttItemLlzViewModelDesignMock(),
                new SdrRttItemLlzViewModelDesignMock(),
                new SdrRttItemLlzViewModelDesignMock(),
            });
        }
    }
    
    public FlightSdrViewModel(ISdrClientDevice payload, ILogService log, ILocalizationService loc, IConfiguration configuration, IEnumerable<ISdrRttItemProvider> rttItems)
        :base(payload, GenerateUri(payload))
    {
        if (rttItems == null) throw new ArgumentNullException(nameof(rttItems));
        _providers = rttItems.ToArray();
        _logService = log ?? throw new ArgumentNullException(nameof(log));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
       
        
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
        
       
        
        UpdateMode = ReactiveCommand.CreateFromTask(cancel=>Payload.Sdr.SetModeAndCheckResult(SelectedMode.Mode,Frequency,1,1,cancel));
        UpdateMode.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error("Set mode",$"Error to set payload mode",ex); // TODO: Localize
        }).DisposeItWith(Disposable);
        
        StartRecord = ReactiveCommand.CreateFromTask(RecordStartImpl,
            this.WhenAnyValue(_=>_.IsRecordStarted).Select(_=>!_));
        StartRecord.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error("Rec start",$"Error to set payload mode",ex);
        }).DisposeItWith(Disposable);
        
        StopRecord = ReactiveCommand.CreateFromTask(cancel=>Payload.Sdr.StopRecordAndCheckResult(cancel),
            this.WhenAnyValue(_=>_.IsRecordStarted));
        StopRecord.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error("Rec stop",$"Error to set payload mode",ex);
        }).DisposeItWith(Disposable);

        Disposable.AddAction(() =>
        {
            foreach (var item in _rttItems)
            {
                item.Dispose();
            }
            _rttItems.Clear();
        });
        
        LinkQuality = new LinkQualitySdrRttViewModel(payload);
    }

    private async Task RecordStartImpl(CancellationToken cancel)
    {
        var dialog = new ContentDialog()
        {
            Title = "New",
            PrimaryButtonText = "Start",
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "Cancel"
        };
            
        using var viewModel = new RecordStartViewModel();
            
        viewModel.ApplyDialog(dialog);
            
        dialog.Content = viewModel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await Payload.Sdr.StartRecordAndCheckResult(viewModel.RecordName, cancel);
            
            viewModel.Tags.ForEach(async _ =>
            {
                if (_ is LongTagViewModel longTag)
                {
                    await Payload.Sdr.CurrentRecordSetTagAndCheckResult(longTag.Name, longTag.Value, new CancellationToken());
                }
                else if (_ is ULongTagViewModel ulongTag)
                {
                    await Payload.Sdr.CurrentRecordSetTagAndCheckResult(ulongTag.Name, ulongTag.Value, new CancellationToken());
                }
                else if (_ is DoubleTagViewModel doubleTag)
                {
                    await Payload.Sdr.CurrentRecordSetTagAndCheckResult(doubleTag.Name, doubleTag.Value, new CancellationToken());
                }
                else if (_ is StringTagViewModel stringTag)
                {
                    await Payload.Sdr.CurrentRecordSetTagAndCheckResult(stringTag.Name, stringTag.Value, new CancellationToken());
                }
            });
        }
    }
    private void UpdateSelectedMode(AsvSdrCustomMode mode)
    {
        SelectedMode = Modes.FirstOrDefault(__ => __.Mode == mode);
        foreach (var item in _rttItems)
        {
            item.Dispose();
        }
        _rttItems.Clear();

        var items = _providers
            .SelectMany(_ => _.Create(Payload, Payload.Sdr.CustomMode.Value))
            .OrderBy(_ => _.Order);
        _rttItems.AddRange(items);


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

    public ObservableCollection<ISdrRttItem> RttItems => _rttItems;
    public ObservableCollection<SdrModeViewModel> Modes { get; } = new();
    [Reactive]
    public SdrModeViewModel? SelectedMode { get; set; }
    [Reactive]
    public ulong Frequency { get; set; }
    [Reactive]
    public bool IsRecordStarted { get; set; }
    
    [Reactive]
    public SdrRttItem LinkQuality { get; set; }
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