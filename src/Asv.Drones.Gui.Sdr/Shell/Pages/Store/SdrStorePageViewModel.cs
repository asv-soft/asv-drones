using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrStorePageViewModel:ShellPage
{
    private readonly ILogService _log;
    private readonly ILocalizationService _loc;
    private readonly ISdrStoreService _store;
    public const string UriString = "asv:shell.page.sdr-store";
    public static readonly Uri Uri = new (UriString);
    private readonly ReadOnlyObservableCollection<SdrDeviceViewModel> _devices;

    public SdrStorePageViewModel() : base(UriString)
    {
        Title = "SDR Records";
        if (Design.IsDesignMode)
        {
            Store = new SdrStoreBrowserViewModel();
            Device = new SdrPayloadBrowserViewModel();
           
        }
    }

    [ImportingConstructor]
    public SdrStorePageViewModel(ILogService log, ILocalizationService loc, ISdrStoreService store, IMavlinkDevicesService mavlink):this()
    {
        _log = log;
        _loc = loc;
        _store = store;
        Store = new SdrStoreBrowserViewModel(store, log);
        Device = new SdrPayloadBrowserViewModel(mavlink, loc, log);
        DownloadRecord = ReactiveCommand.CreateFromTask(DownloadRecordImpl)
            .DisposeItWith(Disposable);
    }

    private async Task<Unit> DownloadRecordImpl(CancellationToken cancel)
    {
        var ifc = Device.SelectedDevice.Client.Sdr.Base;
        var rec = Device.SelectedDevice.SelectedRecord.Record;
        var recId = rec.Id;
        var count = rec.DataCount.Value;
        var recType = rec.DataType.Value;
        var recTypeAsUInt = (uint)recType;
        var take = 10U;
       
        using var writer = _store.Open(recId);
        
        // TODO: add progress bar
        
        var tags = new List<AsvSdrClientRecordTag>();
        rec.Tags.Clone(tags);
        if (rec.TagsCount.Value != tags.Count)
        {
            // update tags
            await rec.DownloadTagList(new Progress<double>(x => { }),cancel);
            writer.EditMetadata(rec.CopyTo);
        }
        foreach (var chunk in writer.GetEmptyChunks(10))
        {
            var downloadRecords = chunk.Take;
            var tcs = new TaskCompletionSource();
            using var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, DisposeCancel);
            using var c1 = linkedCancel.Token.Register(() => { tcs.TrySetCanceled(); });
            linkedCancel.CancelAfter(TimeSpan.FromMilliseconds(5000));
            ifc.OnRecordData.Where(x=>x.MessageId == recTypeAsUInt).Subscribe(x =>
            {   
                writer.Write(x);
                --downloadRecords;
                if (downloadRecords == 0)
                {
                    tcs.TrySetResult();
                }
            },linkedCancel.Token);
            var result = await ifc.GetRecordDataList(recId, chunk.Skip, chunk.Take, cancel);
            if (result.Result != AsvSdrRequestAck.AsvSdrRequestAckOk)
            {
                throw new Exception($"Error download record data: {result.Result}");
            }
            await tcs.Task;
        }
        
        return Unit.Default;

    }

    private void SaveRecord(IPacketV2<IPayload> payload)
    {
        
    }
    public SdrStoreBrowserViewModel Store { get; }
    
    public SdrPayloadBrowserViewModel Device { get; set; }
    public ReactiveCommand<Unit,Unit> DownloadRecord { get; }
}


