using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class PlaningMissionViewModel:PlaningVehicleWidgetBase
    {
        private readonly ILogService _log;
        private readonly ReadOnlyObservableCollection<PlaningMissionItemViewModel> _items;


        /// <summary>
        /// This constructor is used by design time tools
        /// </summary>
        public PlaningMissionViewModel() :base()
        {
            if (Design.IsDesignMode)
            {
                _items = new ReadOnlyObservableCollection<PlaningMissionItemViewModel>(
                    new ObservableCollection<PlaningMissionItemViewModel>(new[]
                    {
                        new PlaningMissionItemViewModel(),
                        new PlaningMissionItemViewModel(),
                        new PlaningMissionItemViewModel(),
                    }));
            }
        }
        
        public PlaningMissionViewModel(IVehicle vehicle,ILogService log) : base(vehicle, "planing-mission")
        {
            _log = log;
            
            _upload = ReactiveCommand.CreateFromObservable(
                () => Observable.FromAsync(UploadImpl).SubscribeOn(RxApp.TaskpoolScheduler).TakeUntil(_cancelUpload), 
                this.WhenAnyValue(_ => _.IsInProgress).Select(_ => !_))
                .DisposeItWith(Disposable);
            _upload.IsExecuting.ToProperty(this, _ => _.IsUploading, out _isUploading)
                .DisposeItWith(Disposable);;
            _upload.ThrownExceptions.Subscribe(OnUploadError)
                .DisposeItWith(Disposable);
            
            _cancelUpload = ReactiveCommand.Create(() => { }, _upload.IsExecuting)
                .DisposeItWith(Disposable);

            _download = ReactiveCommand.CreateFromObservable(
                () => Observable.FromAsync(DownloadImpl).SubscribeOn(RxApp.TaskpoolScheduler).TakeUntil(_cancelDownload), 
                this.WhenAnyValue(_ => _.IsInProgress).Select(_ => !_))
                .DisposeItWith(Disposable);
            _download.IsExecuting.ToProperty(this, _ => _.IsDownloading, out _isDownloading)
                .DisposeItWith(Disposable);;
            _download.ThrownExceptions.Subscribe(OnDownloadError)
                .DisposeItWith(Disposable);;
            _cancelDownload = ReactiveCommand.Create(() => { }, _download.IsExecuting)
                .DisposeItWith(Disposable);;
                
            _clear = ReactiveCommand.CreateFromObservable(
                () => Observable.FromAsync(ClearImpl).SubscribeOn(RxApp.TaskpoolScheduler).TakeUntil(_cancelClear), 
                this.WhenAnyValue(_=>_.IsInProgress).Select(_=>!_) )
                .DisposeItWith(Disposable);
            _clear.IsExecuting.ToProperty(this, _ => _.IsClearing, out _isClearing)
                .DisposeItWith(Disposable);;
            _clear.ThrownExceptions.Subscribe(OnClearError)
                .DisposeItWith(Disposable);;
            _cancelClear = ReactiveCommand.Create(() => { }, _clear.IsExecuting)
                .DisposeItWith(Disposable);;
            
            vehicle.MissionItems
                .Transform(_=>new PlaningMissionItemViewModel(Id,_,this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);

            this.WhenAnyValue(_=>_.IsDownloading,_=>_.IsUploading, _=>_.IsClearing)
                .Select(_=>_.Item1 | _.Item2 | _.Item3)
                .Subscribe(_=> IsInProgress = _)
                .DisposeItWith(Disposable);;
        }

        

        protected override void InternalAfterMapInit(IMap map)
        {
            base.InternalAfterMapInit(map);
            // TODO: Localize
            Vehicle.Class.Subscribe(_ => Icon = MavlinkHelper.GetIcon(_)).DisposeItWith(Disposable);
            Vehicle.Name.Subscribe(_ => Title = $"{_} mission")
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedItem)
                .Where(_=>_!=null)
                .Select(_ => Map.Markers.Where(_ => _ is UavPlaningMissionAnchor)
                    .Cast<UavPlaningMissionAnchor>()
                    .FirstOrDefault(__ => __.MissionItem == _.Item))
                .Where(_=>_!=null)
                .Subscribe(_ => Map.SelectedItem = _)
                .DisposeItWith(Disposable);
            
            Map.WhenAnyValue(_ => _.SelectedItem)
                .Where(_=>_ is UavPlaningMissionAnchor)
                .Cast<UavPlaningMissionAnchor>()
                .Subscribe(_=>SelectedItem = _items.FirstOrDefault(__=>__.Item == _.MissionItem))
                .DisposeItWith(Disposable);
        }

        [Reactive]
        public bool IsInProgress { get; set; }
        [Reactive]
        public double Progress { get; set; }
       
        
        [Reactive]
        public PlaningMissionItemViewModel SelectedItem { get; set; }
        public ReadOnlyObservableCollection<PlaningMissionItemViewModel> Items => _items;
        
        #region Download
            
        private readonly ObservableAsPropertyHelper<bool> _isDownloading;
        public bool IsDownloading => _isDownloading.Value;
        public readonly ReactiveCommand<Unit,Unit> _download;
        public ICommand Download => _download;
        private readonly ReactiveCommand<Unit,Unit> _cancelDownload;
        public ICommand CancelDownload => _cancelDownload;

        private void OnDownloadError(Exception exception)
        {   
            //TODO: Localize
            _log.Error(Title, $"Download mission error {Vehicle.Name.Value}", exception);
        }

        private async Task DownloadImpl(CancellationToken cancel)
        {
            await Vehicle.DownloadMission(3, cancel, _ => Progress = _);
        }

        #endregion
        
        #region Upload

        private readonly ObservableAsPropertyHelper<bool> _isUploading;
        public bool IsUploading => _isUploading.Value;

        public readonly ReactiveCommand<Unit,Unit> _upload;
        public ICommand Upload => _upload;
        public readonly ReactiveCommand<Unit,Unit> _cancelUpload;
        public ICommand CancelUpload => _cancelUpload;

        protected void OnUploadError(Exception exception)
        {
            //TODO: Localize
            _log.Error(Title, $"Upload mission error {Vehicle.Name.Value}", exception);
        }

        protected async Task UploadImpl(CancellationToken cancel)
        {
            // var items = await _vehicle.Mavlink.Mission.MissionRequest(3, cancel);
            await Vehicle.UploadMission(3, cancel, _=>Progress = _/2.0);
            await Vehicle.DownloadMission(3, cancel, _ => Progress = 0.5+_/2.0);
        }

        #endregion

        #region Clear

        private readonly ObservableAsPropertyHelper<bool> _isClearing;
        


        public bool IsClearing => _isClearing.Value;
        private readonly ReactiveCommand<Unit,Unit> _clear;
        public ICommand Clear => _clear;
        private readonly ReactiveCommand<Unit,Unit> _cancelClear;
        public ICommand CancelClear => _cancelClear;

        private void OnClearError(Exception exception)
        {
            //TODO: Localize
            _log.Error(Title, $"Clear mission error {Vehicle.Name.Value}", exception);
        }

        private async Task ClearImpl(CancellationToken cancel)
        {
            await Vehicle.ClearRemoteMission(3, cancel);
        }

        #endregion

        public void NavigateMapToMe(PlaningMissionItemViewModel item)
        {
            if (Map == null) return;
            Map.Center = item.Item.Location.Value;
        }
    }
}