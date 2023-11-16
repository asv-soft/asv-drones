using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
using DocumentFormat.OpenXml.Wordprocessing;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui.Sdr;

public class FlightSdrViewModelConfig
{
    public string GpFrequencyInMhz { get; set; } = "108";
    public string LlzFrequencyInMhz { get; set; } = "108";
    public string VorFrequencyInMhz { get; set; } = "108";

    public string LlzChannel { get; set; }
    public string VorChannel { get; set; }

    public float WriteFrequency { get; set; } = 5;
    public uint ThinningFrequency { get; set; } = 5;
}

public class FlightSdrViewModel:FlightSdrWidgetBase
{
    private readonly ISdrClientDevice _payload;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    private readonly ISdrRttWidgetProvider[] _providers;
    private readonly ObservableCollection<ISdrRttWidget> _rttWidgets = new();
    private readonly IMeasureUnitItem<double,FrequencyUnits> _freqInMHzMeasureUnit;
    private FlightSdrViewModelConfig _config; 
    public static Uri GenerateUri(ISdrClientDevice sdr) => FlightSdrWidgetBase.GenerateUri(sdr,"sdr");

    public FlightSdrViewModel()
    {
        if (Design.IsDesignMode)
        {
            Icon = SdrIconHelper.DefaultIcon;
            _rttWidgets = new ObservableCollection<ISdrRttWidget>(new List<ISdrRttWidget>
            {
                new LlzSdrRttViewModel(),
            });
        }
    }

    public FlightSdrViewModel(ISdrClientDevice payload, ILogService log, ILocalizationService loc,
        IConfiguration configuration, IEnumerable<ISdrRttWidgetProvider> rtt) : base(payload, GenerateUri(payload))
    {
        _providers = rtt.ToArray();
        _payload = payload;
        _logService = log ?? throw new ArgumentNullException(nameof(log));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _config = _configuration.Get<FlightSdrViewModelConfig>();
        _freqInMHzMeasureUnit = _loc.Frequency.AvailableUnits.First(_ => _.Id == Core.FrequencyUnits.MHz);

        
        
        this.WhenValueChanged(_ => _.SelectedMode)
            .Subscribe(SelectedModeChanged).DisposeItWith(Disposable);

        this.WhenValueChanged(_ => _.Channel)
            .Subscribe(_ =>
            {
                if (_ == null) return;
                if (SelectedMode == null) return;
                switch (SelectedMode.Mode)
                {
                    case AsvSdrCustomMode.AsvSdrCustomModeLlz:
                        FrequencyInMhz =
                            _freqInMHzMeasureUnit.FromSiToString(
                                SdrRttHelper.GetLocalizerModeFrequencyFromIlsChannel(_));
                        break;
                    case AsvSdrCustomMode.AsvSdrCustomModeVor:
                        FrequencyInMhz =
                            _freqInMHzMeasureUnit.FromSiToString(SdrRttHelper.GetVorFrequencyFromVorChannel(_));
                        break;
                    case AsvSdrCustomMode.AsvSdrCustomModeGp:
                    case AsvSdrCustomMode.AsvSdrCustomModeIdle:
                    default:
                        break;
                }
            }).DisposeItWith(Disposable);
        
        Icon = SdrIconHelper.DefaultIcon;
        payload.Name.Subscribe(x=>Title = x).DisposeItWith(Disposable);
        Location = WidgetLocation.Right;

        payload.Sdr.SupportedModes.DistinctUntilChanged()
            .Subscribe(UpdateModes)
            .DisposeItWith(Disposable);
        payload.Sdr.CustomMode.DistinctUntilChanged()
            .Subscribe(UpdateSelectedMode)
            .DisposeItWith(Disposable);
        payload.Sdr.IsRecordStarted.DistinctUntilChanged()
            .Subscribe(x=>IsRecordStarted = x)
            .DisposeItWith(Disposable);
        
        UpdateMode = ReactiveCommand.CreateFromTask(cancel =>
        {
            if (SelectedMode == null) return Task.CompletedTask;
            switch (SelectedMode.Mode)
            {
                case AsvSdrCustomMode.AsvSdrCustomModeGp: 
                    _config.GpFrequencyInMhz = FrequencyInMhz;
                    break;
                case AsvSdrCustomMode.AsvSdrCustomModeLlz:
                    _config.LlzChannel = Channel;
                    _config.LlzFrequencyInMhz = FrequencyInMhz;
                    break;
                case AsvSdrCustomMode.AsvSdrCustomModeVor: 
                    _config.VorChannel = Channel;
                    _config.VorFrequencyInMhz = FrequencyInMhz;
                    break;
                case AsvSdrCustomMode.AsvSdrCustomModeIdle: default:
                    break;
            }
            _configuration.Set(_config);
               return Payload.Sdr.SetModeAndCheckResult(SelectedMode.Mode,
                (ulong)Math.Round(_freqInMHzMeasureUnit.ConvertToSi(FrequencyInMhz)), _config.WriteFrequency, _config.ThinningFrequency, cancel);
        });
        UpdateMode.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error(RS.FlightSdrViewModel_UpdateMode_Error_Sender,RS.FlightSdrViewModel_UpdateMode_Error_Message,ex);
        }).DisposeItWith(Disposable);
        
        StartRecord = ReactiveCommand.CreateFromTask(RecordStartImpl,
            this.WhenAnyValue(_=>_.IsRecordStarted).Select(_=>!_));
        StartRecord.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error(RS.FlightSdrViewModel_StartRecord_Error_Sender,RS.FlightSdrViewModel_StartRecord_Error_Message,ex);
        }).DisposeItWith(Disposable);
        
        StopRecord = ReactiveCommand.CreateFromTask(cancel=>Payload.Sdr.StopRecordAndCheckResult(cancel),
            this.WhenAnyValue(_=>_.IsRecordStarted));
        StopRecord.ThrownExceptions.Subscribe(ex =>
        {
            _logService.Error(RS.FlightSdrViewModel_StopRecord_Error_Sender,RS.FlightSdrViewModel_StopRecord_Error_Message,ex);
        }).DisposeItWith(Disposable);

        Disposable.AddAction(() =>
        {
            foreach (var item in _rttWidgets)
            {
                item.Dispose();
            }
            _rttWidgets.Clear();
        });
        
        LinkQuality = new LinkQualitySdrRttViewModel(payload);
        
        SafeRebootOSCommand = ReactiveCommand.CreateFromTask(cancel =>
            payload.Sdr.SystemControlAction(AsvSdrSystemControlAction.AsvSdrSystemControlActionReboot, cancel));
        SafeShutdownOSCommand = ReactiveCommand.CreateFromTask(cancel =>
            payload.Sdr.SystemControlAction(AsvSdrSystemControlAction.AsvSdrSystemControlActionShutdown, cancel));
        
       
        this.ValidationRule(x => x.FrequencyInMhz,
                _ => _loc.Frequency.IsValid(_) && _loc.Frequency.ConvertToSi(_) > 0,
                RS.FlightSdrViewModel_Frequency_Validation_ErrorMessage)
            .DisposeItWith(Disposable);
        
        _payload.Sdr.Base.Status.Select(_=>_.MissionState)
            .Subscribe(_=>IsMissionStarted = _ == AsvSdrMissionState.AsvSdrMissionStateProgress)
            .DisposeItWith(Disposable);
        
        UpdateMission = new CancellableCommandWithProgress<Unit,Unit>(UpdateMissionImpl, "Mission update", log, Scheduler.Default).DisposeItWith(Disposable);

        StartMission = ReactiveCommand.CreateFromTask<Unit,Unit>(StartMissionImpl);
        StartMission.ThrownExceptions.Subscribe(_=>_logService.Error(Title, "Start mission failed", _)).DisposeItWith(Disposable);
        StopMission = ReactiveCommand.CreateFromTask<Unit,Unit>(StopMissionImpl);
        StopMission.ThrownExceptions.Subscribe(_=>_logService.Error(Title, "Stop mission failed", _)).DisposeItWith(Disposable);
    }

   


    #region Mission

    public ReactiveCommand<Unit,Unit> StartMission { get; }

    private async Task<Unit> StartMissionImpl(Unit args, CancellationToken cancel)
    {
        await _payload.Sdr.StartMission(0, cancel);
        return Unit.Default;
    }
    public ReactiveCommand<Unit,Unit> StopMission { get; }
    private async Task<Unit> StopMissionImpl(Unit args, CancellationToken cancel)
    {
        await _payload.Sdr.StopMission(cancel);
        return Unit.Default;
    }
    private async Task<Unit> UpdateMissionImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        MissionStatusText = "Loading mission...";
        var items = await _payload.Missions.Download(cancel);
        return Unit.Default;
    }   

  
    public CancellableCommandWithProgress<Unit,Unit> UpdateMission { get; }
    [Reactive]
    public string MissionStatusText { get; set; }
    [Reactive]
    public bool IsMissionStarted { get; set; }

    #endregion
    
    private void SelectedModeChanged(SdrModeViewModel? _)
    {
        if (_ == null) return;
        switch (_.Mode)
        {
            case AsvSdrCustomMode.AsvSdrCustomModeGp:
                FrequencyInMhz = _config.GpFrequencyInMhz;
                IsIdleMode = false;
                IsGpMode = true;
                break;
            case AsvSdrCustomMode.AsvSdrCustomModeLlz:
                Channels = SdrRttHelper.GetLlzChannels();
                Channel = _config.LlzChannel;
                FrequencyInMhz = _config.LlzFrequencyInMhz;
                IsIdleMode = false;
                IsGpMode = false;
                break;
            case AsvSdrCustomMode.AsvSdrCustomModeVor:
                Channels = SdrRttHelper.GetVorChannels();
                Channel = _config.VorChannel;
                FrequencyInMhz = _config.VorFrequencyInMhz;
                IsIdleMode = false;
                IsGpMode = false;
                break;
            case AsvSdrCustomMode.AsvSdrCustomModeIdle:
            default:
                FrequencyInMhz = "0";
                IsIdleMode = true;
                IsGpMode = false;
                break;
        }
    }
    private async Task RecordStartImpl(CancellationToken cancel)
    {
        var dialog = new ContentDialog()
        {
            Title = RS.FlightSdrViewModel_RecordStartDialog_Title,
            PrimaryButtonText = RS.FlightSdrViewModel_RecordStartDialog_PrimarryButton_Name,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.FlightSdrViewModel_RecordStartDialog_SecondaryButton_Name
        };
            
        var viewModel = new RecordStartViewModel(Payload);
        
        viewModel.RecordName = $"{Modes.First(_ => _.Mode == Payload.Sdr.CustomMode.Value).Name}_{DateTime.Now:yy_MM_dd_hh_mm}";
        
        viewModel.ApplyDialog(dialog);
            
        dialog.Content = viewModel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var startMavResult = MavResult.MavResultUnsupported;
            for (int i = 0; i < 5; i++)
            {
                startMavResult = await Payload.Sdr.StartRecord(viewModel.RecordName, cancel);
                if (cancel.IsCancellationRequested || startMavResult == MavResult.MavResultAccepted) break;
            }
            
            var tagMavResult = MavResult.MavResultUnsupported;
            
            for (int i = 0; i < 5; i++)
            {
                tagMavResult = await Payload.Sdr.CurrentRecordSetTag("Kit", 
                    viewModel.SelectedKit, cancel).ConfigureAwait(false);
                if (cancel.IsCancellationRequested || tagMavResult == MavResult.MavResultAccepted) break;
            }
            
            tagMavResult = MavResult.MavResultUnsupported;
            
            for (int i = 0; i < 5; i++)
            {
                tagMavResult = await Payload.Sdr.CurrentRecordSetTag("Mission", 
                    viewModel.SelectedMission, cancel).ConfigureAwait(false);
                if (cancel.IsCancellationRequested || tagMavResult == MavResult.MavResultAccepted) break;
            }

            if (cancel.IsCancellationRequested) return;
            
            if (startMavResult == MavResult.MavResultAccepted)
            {
                foreach (var tag in viewModel.Tags)
                {
                    tagMavResult = MavResult.MavResultUnsupported;
                    
                    if (tag is LongTagViewModel longTag)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            tagMavResult = await Payload.Sdr.CurrentRecordSetTag(longTag.Name, 
                                longTag.Value, cancel).ConfigureAwait(false);
                            if (cancel.IsCancellationRequested || tagMavResult == MavResult.MavResultAccepted) break;
                        }
                        if (!cancel.IsCancellationRequested && tagMavResult != MavResult.MavResultAccepted)
                            _logService.Error(Title, 
                                $"Long tag {longTag.Name} setup failed. Result: {tagMavResult}");
                    }
                    else if (tag is ULongTagViewModel ulongTag)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            tagMavResult = await Payload.Sdr.CurrentRecordSetTag(ulongTag.Name, 
                                ulongTag.Value, cancel).ConfigureAwait(false);
                            if (cancel.IsCancellationRequested || tagMavResult == MavResult.MavResultAccepted) break;
                        }
                        if (!cancel.IsCancellationRequested && tagMavResult != MavResult.MavResultAccepted)
                            _logService.Error(Title, 
                                $"ULong tag {ulongTag.Name} setup failed. Result: {tagMavResult}");
                    }
                    else if (tag is DoubleTagViewModel doubleTag)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            tagMavResult = await Payload.Sdr.CurrentRecordSetTag(doubleTag.Name, 
                                doubleTag.Value, cancel).ConfigureAwait(false);
                            if (cancel.IsCancellationRequested || tagMavResult == MavResult.MavResultAccepted) break;
                        }
                        if (!cancel.IsCancellationRequested && tagMavResult != MavResult.MavResultAccepted)
                            _logService.Error(Title, 
                                $"Double tag {doubleTag.Name} setup failed. Result: {tagMavResult}");
                    }
                    else if (tag is StringTagViewModel stringTag)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            tagMavResult = await Payload.Sdr.CurrentRecordSetTag(stringTag.Name, 
                                stringTag.Value, cancel).ConfigureAwait(false);
                            if (cancel.IsCancellationRequested || tagMavResult == MavResult.MavResultAccepted) break;
                        }
                        if (!cancel.IsCancellationRequested && tagMavResult != MavResult.MavResultAccepted)
                            _logService.Error(Title, 
                                $"String tag {stringTag.Name} setup failed. Result: {tagMavResult}");
                    }
                    if (cancel.IsCancellationRequested) break;
                }
            }
            else
            {
                _logService.Error(Title, $"Start record failed. Result: {startMavResult}");
            }
        }
    }
    private void UpdateSelectedMode(AsvSdrCustomMode mode)
    {
        SelectedMode = Modes.FirstOrDefault(__ => __.Mode == mode);
        foreach (var item in _rttWidgets)
        {
            item.Dispose();
        }
        _rttWidgets.Clear();

        var items = _providers
            .Select(_ => _.Create(Payload, Payload.Sdr.CustomMode.Value))
            .IgnoreNulls()
            .OrderBy(_ => _.Order);
        _rttWidgets.AddRange(items);
        IsRecordVisible = mode != AsvSdrCustomMode.AsvSdrCustomModeIdle;

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

    public ObservableCollection<ISdrRttWidget> RttWidgets => _rttWidgets;
    public string FrequencyUnits => _freqInMHzMeasureUnit.Unit;
    public ObservableCollection<SdrModeViewModel> Modes { get; } = new();
    [Reactive]
    public SdrModeViewModel? SelectedMode { get; set; }
    [Reactive]
    public string FrequencyInMhz { get; set; }
    
    [Reactive]
    public string Channel { get; set; }
    
    [Reactive]
    public bool IsRecordStarted { get; set; }
    
    [Reactive]
    public bool IsIdleMode { get; set; }
    
    [Reactive]
    public bool IsRecordVisible { get; set; }
    
    [Reactive]
    public IEnumerable<string> Channels { get; set; }
    
    [Reactive]
    public bool IsGpMode { get; set; }
    
    [Reactive]
    public SdrRttItem LinkQuality { get; set; }
    public ReactiveCommand<Unit,Unit> UpdateMode { get; }
    public ReactiveCommand<Unit,Unit> StartRecord { get; }
    public ReactiveCommand<Unit,Unit> StopRecord { get; }
    public ICommand SafeRebootOSCommand { get; set; }
    public ICommand SafeShutdownOSCommand { get; set; }
   
}
public class SdrModeViewModel:ReactiveObject
{
    public MaterialIconKind Icon { get; }
    public string Name { get; }
    public AsvSdrCustomMode Mode { get; }

    public SdrModeViewModel(string name, AsvSdrCustomMode mode, MaterialIconKind icon)
    {
        Name = name;
        Mode = mode;
        Icon = icon;
    }
}
