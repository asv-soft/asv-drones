using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrPayloadBrowserViewModel:ViewModelBase
{
    private readonly ILocalizationService _loc;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<SdrDeviceViewModel> _devices;


    public const string UriString = "asv:sdr.device.browser";

    public SdrPayloadBrowserViewModel():base(UriString)
    {
        if (Design.IsDesignMode)
        {
            _devices = new ReadOnlyObservableCollection<SdrDeviceViewModel>(
                new ObservableCollection<SdrDeviceViewModel>(new List<SdrDeviceViewModel>
                {
                    new(110),
                    new(120),
                    new(130),
                }));
        }
    }
    
    public SdrPayloadBrowserViewModel(IMavlinkDevicesService mavlink, ILocalizationService loc, ILogService log):this()
    {
        _loc = loc;
        _log = log;
        mavlink
            .Payloads
            .Transform(_ => new SdrDeviceViewModel(_,loc,log))
            .Bind(out _devices)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_=>SelectedDevice)
            .Where(_=>_ != null)
            .Subscribe(_=>_.DownloadRecords.Execute().Subscribe())
            .DisposeItWith(Disposable);
    }

    public ReadOnlyObservableCollection<SdrDeviceViewModel> Devices => _devices;
    
    [Reactive]
    public SdrDeviceViewModel? SelectedDevice { get; set; }

    public void TrySelect(Guid recordId)
    {
        SelectedDevice?.TrySelect(recordId);
    }
}

public class SdrDeviceViewModel:ViewModelBase
{
    private readonly ILocalizationService _loc;
    private readonly ILogService _log;
    private readonly ReadOnlyObservableCollection<SdrPayloadRecordViewModel> _items;

    public SdrDeviceViewModel(ushort id):base($"asv:shell.page.sdr-store.device?id={id}")
    {
        Name = $"Payload {id}";
        if (Design.IsDesignMode)
        {
            _items = new ReadOnlyObservableCollection<SdrPayloadRecordViewModel>(new ObservableCollection<SdrPayloadRecordViewModel>(new List<SdrPayloadRecordViewModel>
            {
                new(Guid.NewGuid())
                {
                    Name = "Record1",
                    CreatedDateTime = DateTime.Now.AddMinutes(-60),
                },
                new(Guid.NewGuid())
                {
                    Name = "Record2",
                    CreatedDateTime = DateTime.Now.AddMinutes(-50),
                },
                new(Guid.NewGuid())
                {
                    Name = "Record3",
                    CreatedDateTime = DateTime.Now.AddMinutes(-40),
                }
            }));
        }
    }
    public SdrDeviceViewModel(ISdrClientDevice device, ILocalizationService loc,ILogService log):this(device.Heartbeat.FullId)
    {
        _loc = loc;
        _log = log;
        Client = device;
        device.Sdr.Records
            .Transform(_=>new SdrPayloadRecordViewModel(device.Heartbeat.FullId,_,_log,_loc))
            .SortBy(_=>_.CreatedDateTime)
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        this.WhenValueChanged(_ => SelectedRecord)
            .Where(_=>_ != null)
            .Subscribe(_=>_.DownloadTags.Execute().Subscribe())
            .DisposeItWith(Disposable);
        
        DownloadRecords = ReactiveCommand.CreateFromTask(cancel =>
                device.Sdr.DownloadRecordList(new Progress<double>(_ => DownloadRecordsProgress = _), cancel))
            .DisposeItWith(Disposable);
        
        DownloadRecords.ThrownExceptions.Subscribe(_ => _log.Error("Record", "Error to download records", _))
            .DisposeItWith(Disposable);
    }

    [Reactive]
    public double DownloadRecordsProgress { get; set; }
    public ReactiveCommand<Unit,bool> DownloadRecords { get; }

    public string Name { get; set; }
    
    public ReadOnlyObservableCollection<SdrPayloadRecordViewModel> Items => _items;
    
    [Reactive]
    public SdrPayloadRecordViewModel SelectedRecord { get; set; }

    public ISdrClientDevice Client { get; }

    public void TrySelect(Guid recordId)
    {
        var item = Items.FirstOrDefault(x => x.Record.Id == recordId);
        if (item == null) return;
        SelectedRecord = item;
    }
}



