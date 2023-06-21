using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Web;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrViewModel:ViewModelBase,IShellPage
{
    public const string UriString = "asv:shell.page.sdr";
    public static Uri GenerateUri(ushort id) => new($"{UriString}?id={id}");
   
    private readonly IMavlinkDevicesService _mavlink;
    private readonly ILogService _log;
    private readonly ILocalizationService _loc;

    
    private ISdrClientDevice? _payload;
    private ReadOnlyObservableCollection<SdrRecordViewModel> _records;
    private ObservableAsPropertyHelper<bool> _isExecuting;
    
    public SdrViewModel() : base(UriString)
    {
        if (Design.IsDesignMode)
        {
            _records = new ReadOnlyObservableCollection<SdrRecordViewModel>(
                new ObservableCollection<SdrRecordViewModel>(
                    new List<SdrRecordViewModel>
                    {
                        new() {Name = "Record1"},
                        new() {Name = "Record2"},
                        new() {Name = "Record3"},
                    }));
        }
    }

    [ImportingConstructor]
    public SdrViewModel(IMavlinkDevicesService mavlink, ILogService log, ILocalizationService loc) : base(UriString)
    {
        _mavlink = mavlink ?? throw new ArgumentNullException(nameof(mavlink));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _loc = loc;
    }

    public void SetArgs(Uri link)
    {
        var query =  HttpUtility.ParseQueryString(link.Query);
        if (ushort.TryParse(query["id"], out var id) == false) return;
        
        _payload = _mavlink.GetPayloadsByFullId(id);
        if (_payload == null) return;
        
        _payload.Sdr.Records
            .Transform(_=>new SdrRecordViewModel(_payload.Heartbeat.FullId,_,_log,_loc))
            .SortBy(_=>_.CreatedDateTime)
            .Bind(out _records)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        
        DownloadRecords = ReactiveCommand.CreateFromTask(cancel =>
                _payload.Sdr.DownloadRecordList(new Progress<double>(_ => Progress = _), cancel))
            .DisposeItWith(Disposable);
        DownloadRecords.IsExecuting.ToProperty(this, _ => _.IsExecuting, out _isExecuting)
            .DisposeItWith(Disposable);
        DownloadRecords.ThrownExceptions.Subscribe(_ => _log.Error(Title, "Error to download records", _))
            .DisposeItWith(Disposable);

        
    }

    [Reactive]
    public string Title { get; set; }

    [Reactive]
    public double Progress { get; set; }
    
    public bool IsExecuting => _isExecuting.Value;

    public ReadOnlyObservableCollection<SdrRecordViewModel> Items => _records;
    public ReactiveCommand<Unit,bool> DownloadRecords { get; set; }
    [Reactive]
    public SdrRecordViewModel? SelectedItem { get; set; }
}