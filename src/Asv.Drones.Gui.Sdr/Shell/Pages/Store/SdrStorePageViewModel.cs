using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class SdrStorePageViewModelConfig
{
    public int PageSizeToDownloadRecords { get; set; } = 5;
    public int DownloadRecordResponseTimeoutMs { get; set; } = 300;
}

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrStorePageViewModel:ShellPage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ILogService _log;
    private readonly ILocalizationService _loc;
    private readonly ISdrStoreService _store;
    public const string UriString = "asv:shell.page.sdr-store";
    public static readonly Uri Uri = new (UriString);
    private readonly ReadOnlyObservableCollection<SdrDeviceViewModel> _devices;
    private readonly SdrStorePageViewModelConfig _config;

    public SdrStorePageViewModel() : base(UriString)
    {
        Title = "SDR Records";
        if (Design.IsDesignMode)
        {
            Store = new SdrStoreBrowserViewModel();
            Device = new SdrPayloadBrowserViewModel();
            DownloadRecordCaption = "Dwonload asdasdads.sdr";
            DownloadRecordAction = "Read record";

        }
    }

    [ImportingConstructor]
    public SdrStorePageViewModel(ILogService log, ILocalizationService loc, ISdrStoreService store, IMavlinkDevicesService mavlink, IConfiguration config):this()
    {
        _log = log;
        _loc = loc;
        _store = store;
        _config = config.Get<SdrStorePageViewModelConfig>();
        Store = new SdrStoreBrowserViewModel(store, loc, log);
        Device = new SdrPayloadBrowserViewModel(mavlink, loc, log);
        DownloadRecord = new CancellableCommandWithProgress<Unit, Unit>(DownloadRecordImpl,"Download record",log).DisposeItWith(Disposable);
        Store.WhenValueChanged(_ => _.SelectedItem).Subscribe(TrySelectDeviceItem).DisposeItWith(Disposable);
        Device.WhenValueChanged(_ => _.SelectedDevice!.SelectedRecord).Subscribe(TrySelectStoreItem).DisposeItWith(Disposable);
        
    }

    private void TrySelectDeviceItem(HierarchicalStoreEntryViewModel? sdrStoreEntityViewModel)
    {
        if (sdrStoreEntityViewModel == null) return;
        Store.TrySelect(sdrStoreEntityViewModel.Id);
    }

    private void TrySelectStoreItem(SdrPayloadRecordViewModel? sdrPayloadRecordViewModel)
    {
        if (sdrPayloadRecordViewModel == null) return;
        Device.TrySelect(sdrPayloadRecordViewModel.Record.Id);
    }

    public CancellableCommandWithProgress<Unit,Unit> DownloadRecord { get; private set; }

    private async Task<Unit> DownloadRecordImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        
        var ifc = Device.SelectedDevice.Client.Sdr.Base;
        var rec = Device.SelectedDevice.SelectedRecord.Record;
        var recId = rec.Id;
        var count = rec.DataCount.Value;
        var recType = rec.DataType.Value;
        var recTypeAsUInt = (uint)recType;
        var take = _config.PageSizeToDownloadRecords;

        DownloadRecordCaption = $"Download {rec.Name}";
        DownloadRecordAction = "Download tags";
        
        await rec.DownloadTagList(progress,cancel);
        
        var parent = Store.SelectedItem == null ? _store.Store.RootFolderId :
            Store.SelectedItem.IsFile ? Store.SelectedItem.ParentId : Store.SelectedItem.Id;

        using var writer = 
            _store.Store.FileExists(rec.Id) 
                ? _store.Store.OpenFile(rec.Id) 
                : _store.Store.CreateFile(recId, rec.Name.Value, (Guid)parent);
        
        rec.CopyMetadataTo(writer.File);        
        
        var remoteCount = rec.DataCount.Value;
        
        if (writer.File.Count < remoteCount)
        {
            Logger.Debug($"Remote count({remoteCount}) != local count({writer.File.Count}). Try to read last item");
            while (cancel.IsCancellationRequested == false)
            {
                var result = await ReadChunk(ifc, new ListDataFileHelper.Chunk { Skip = remoteCount - 1, Take = 1 }, cancel, writer.File,
                    recTypeAsUInt, recId);
                if (result == 1) break;
            }
        }

        var alreadyDownloaded = 0;
        for (int i = 0; i < writer.File.Count; i++)
        {
            if (writer.File.Exist((uint)i))
            {
                ++alreadyDownloaded;
            }
        }
        progress.Report((alreadyDownloaded / (double)remoteCount));
        foreach (var emptyChunk in GetEmptyChunks(writer.File,take))
        {
            var readed = await ReadChunk(ifc, emptyChunk,cancel,writer.File, recTypeAsUInt,recId);
            alreadyDownloaded += readed;
            progress.Report((alreadyDownloaded / (double)remoteCount));
            DownloadRecordAction = $"Read {alreadyDownloaded} of {writer.File.Count}";
        }
        await Store.Refresh.Execute();
        return Unit.Default;
    }

    private IEnumerable<ListDataFileHelper.Chunk> GetEmptyChunks(IListDataFile<AsvSdrRecordFileMetadata> src, int maxPageSize)
    {
        uint count = src.Count;
        bool startedChunk = false;
        uint skip = 0;
        uint take = 0;
        for (uint i = 0; i < count; ++i)
        {
            if ((long) take >= (long) maxPageSize)
            {
                yield return new ListDataFileHelper.Chunk()
                {
                    Skip = skip,
                    Take = take
                };
                skip = 0;
                take = 0;
                startedChunk = false;
            }
            if (!startedChunk)
            {
                if (!src.Exist(i))
                {
                    skip = i;
                    take = 1U;
                    startedChunk = true;
                }
            }
            else if (src.Exist(i))
            {
                yield return new ListDataFileHelper.Chunk()
                {
                    Skip = skip,
                    Take = take
                };
                skip = 0;
                take = 0;
                startedChunk = false;
            }
            else
                ++take;
        }
    }

    private async Task<int> ReadChunk(IAsvSdrClient ifc, ListDataFileHelper.Chunk chunk, CancellationToken cancel,
        IListDataFile<AsvSdrRecordFileMetadata> file, uint recTypeAsUInt,Guid recId)
    {
        using var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
        var tcs = new TaskCompletionSource<int>();
        await using var c1 = linkedCancel.Token.Register(() => tcs.TrySetCanceled());
        var readCount = 0;
        DateTime lastUpdate = DateTime.Now;
        using var subscribe = ifc.OnRecordData.Where(packetV2=>packetV2.MessageId == recTypeAsUInt).Subscribe(x =>
        {
            var items = Interlocked.Increment(ref readCount);
            Logger.Trace($"Save {items} record {x.Name}");
            file.Write(x);
            lastUpdate = DateTime.Now;
        });
        Logger.Trace($"Request skip:{chunk.Skip} , take:{chunk.Take}");
        var result = await ifc.GetRecordDataList(recId, chunk.Skip, chunk.Take, cancel);
        if (result.ItemsCount == 0) return readCount;

        using var subscribe2 = Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .Subscribe(_ =>
            {
                if (readCount >= (int)result.ItemsCount)
                {
                    Logger.Trace($"Complete download skip:{chunk.Skip} , take:{chunk.Take}");
                    tcs.TrySetResult(readCount);
                    return;
                }
                if ((DateTime.Now - lastUpdate).TotalMilliseconds > _config.DownloadRecordResponseTimeoutMs)
                {
                    Logger.Trace($"Complete download skip:{chunk.Skip} , take:{chunk.Take}, get {readCount}");
                    tcs.TrySetResult(readCount);
                    return;
                }
            });
        await tcs.Task;
        return readCount;
    }
    
    
    public SdrStoreBrowserViewModel Store { get; }
    
    [Reactive]
    public SdrPayloadBrowserViewModel Device { get; set; }

    [Reactive]
    public string DownloadRecordCaption { get; set; }
    [Reactive]
    public string DownloadRecordAction { get; set; }
}


