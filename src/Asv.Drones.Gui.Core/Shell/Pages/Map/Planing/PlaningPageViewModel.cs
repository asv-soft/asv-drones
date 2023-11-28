using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class PlanningPageViewModelConfig
    {
        public double Zoom { get; set; }
        public GeoPoint MapCenter { get; set; }
    }
    
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningPageViewModel : MapPageViewModel, IPlaningMissionContext
    {
        public const string UriString = "asv:shell.page.map.planing";
        public static readonly Uri Uri = new(UriString);
        private readonly IPlaningMissionAnchorProvider _anchorProvider;
        private readonly IPlaningMissionPointFactory _taskFactory;
        private readonly IMavlinkDevicesService _devices;
        private readonly CompositionContainer _container;
        private readonly IPlaningMission _svc;
        private readonly ILogService _log;
        private ReadOnlyObservableCollection<IHeaderMenuItem> _uploadMenuItems;
        private ReadOnlyObservableCollection<IHeaderMenuItem> _downloadMenuItems;

        [ImportingConstructor]
        public PlaningPageViewModel(IMapService map, IConfiguration cfg, IPlaningMission svc, ILogService log,
            CompositionContainer container, IPlaningMissionPointFactory taskFactory, IMavlinkDevicesService devices,
            [ImportMany(HeaderMenuItem.UriString)] IEnumerable<IHeaderMenuItem> exportedMenuItems,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapWidget>> widgets,
            [ImportMany(UriString)] IEnumerable<IViewModelProvider<IMapAction>> actions):base(Uri,map,exportedMenuItems,markers,widgets,actions)
        {
            Title = RS.PlaningShellMenuItem_Name;
            Icon = MaterialIconKind.MapMarkerCheck;
            PlanningConfig = cfg.Get<PlanningPageViewModelConfig>();

            _svc = svc;
            _log = log;
            _devices = devices;
            _container = container;
            _taskFactory = taskFactory;
            
            _devices.Vehicles
                .ChangeKey(_ => _.FullId)
                .Transform(_ =>
                {
                    var item = new HeaderMenuItem($"{HeaderPlaningFileUploadMenuItem.UriString}/{_.FullId}")
                    {
                        Icon = MavlinkHelper.GetIcon(_.Class),
                        Command = ReactiveCommand.CreateFromTask(cancel => UploadMission(_, null, Mission.Points, cancel))
                            .DisposeItWith(Disposable)
                    };
                    _.Name.Subscribe(_ => item.Header = _);
                    return (IHeaderMenuItem)item;
                })
                .Bind(out _uploadMenuItems)
                .Subscribe()
                .DisposeItWith(Disposable);
            
            _devices.Vehicles
                .ChangeKey(_ => _.FullId)
                .Transform(_ =>
                {
                    var item = new HeaderMenuItem($"{HeaderPlaningFileDownloadMenuItem.UriString}/{_.FullId}")
                    {
                        Icon = MavlinkHelper.GetIcon(_.Class),
                        Command = ReactiveCommand.CreateFromTask(cancel => DownloadMission(_, cancel))
                    };
                    _.Name.Subscribe(_ => item.Header = _);
                    return (IHeaderMenuItem)item;
                })
                .Bind(out _downloadMenuItems)
                .Subscribe()
                .DisposeItWith(Disposable);
            
            var uploadMenuItem = new HeaderPlaningFileUploadMenuItem
            {
                Command = ReactiveCommand.Create(() => { }, this.WhenAnyValue(_ => _.Mission).Select(_ => _ != null)).DisposeItWith(Disposable),
                Items = _uploadMenuItems
            };

            var downloadMenuItem = new HeaderPlaningFileDownloadMenuItem
            {
                Command = ReactiveCommand.Create(() => { }, this.WhenAnyValue(_ => _.Mission).Select(_ => _ != null)).DisposeItWith(Disposable),
                Items = _downloadMenuItems
            };
            
            HeaderItemsSource.AddOrUpdate(
                new HeaderPlaningFileMenuItem
                {
                    Items = new ReadOnlyObservableCollection<IHeaderMenuItem>(new ObservableCollection<IHeaderMenuItem>(new IHeaderMenuItem[]
                    {
                        new HeaderPlaningFileOpenMenuItem
                        {
                            Command = ReactiveCommand.CreateFromTask(OpenBrowserImpl).DisposeItWith(Disposable)
                        },
                        new HeaderPlaningFileSaveMenuItem
                        {
                            Command = ReactiveCommand.Create(() =>
                            {
                                Mission.SaveCmd.Execute().Subscribe();
                            }, this.WhenAnyValue(_ => _.Mission).Select(_ => _ != null)).DisposeItWith(Disposable)
                        },
                        new HeaderPlaningFileDeleteMenuItem
                        {
                            Command = ReactiveCommand.Create(() =>
                            {
                                _svc.MissionStore.DeleteFile(Mission.MissionId);
                                    Mission = null; 
                            }, this.WhenAnyValue(_ => _.Mission).Select(_ => _ != null)).DisposeItWith(Disposable)
                        },
                        uploadMenuItem,
                        downloadMenuItem
                    }))
                });

            foreach (var marker in markers)
            {
                if (marker is IPlaningMissionAnchorProvider anchorProvider)
                {
                    _anchorProvider = anchorProvider;
                }
            }
            
            Zoom = PlanningConfig.Zoom is 0 ? 1 : PlanningConfig.Zoom;

            Center = PlanningConfig.MapCenter;

            this.WhenPropertyChanged(_ => _.Zoom)
                .Subscribe(_ =>
                {
                    PlanningConfig.Zoom = _.Value;
                    cfg.Set(PlanningConfig);
                })
                .DisposeItWith(Disposable);
            
            this.WhenPropertyChanged(_ => _.Center)
                .Subscribe(_ =>
                {
                    PlanningConfig.MapCenter = _.Value;
                    cfg.Set(PlanningConfig);
                })
                .DisposeItWith(Disposable);

            this.WhenPropertyChanged(_ => _.Mission)
                .Subscribe(_ =>
                {   
                    _anchorProvider.Update(_.Value);
                }).DisposeItWith(Disposable);
        }

        private async Task UploadMission(IVehicleClient vehicle, ISdrClientDevice sdr, IEnumerable<PlaningMissionPointViewModel> points, CancellationToken cancel)
        {
            await vehicle.Missions.ClearRemote(cancel);
            
            vehicle.Missions.ClearLocal();
            
            vehicle.Missions.AddNavMissionItem(vehicle.Position.Home.Value ?? GeoPoint.Zero); 
            
            points.ForEach(_ =>
            {
                if(cancel.IsCancellationRequested) return;
                _.CreateVehicleItems(vehicle, sdr);
            });
            
            try
            {
                await vehicle.Missions.Upload(cancel, progress => UploadProgress = progress);
                _log.Info("Planing", RS.PlanningPageViewModel_MissionUploaded);
            }
            catch (Exception ex)
            {
                _log.Error("Planing", ex.Message);
            }
        }

        private async Task DownloadMission(IVehicleClient vehicle, CancellationToken cancel)
        {
            Mission?.ClearAllPoints();
            var vehicleMission = await vehicle.Missions.Download(cancel);
            foreach (var missionItem in vehicleMission)
            {
                if(cancel.IsCancellationRequested) return;
                if (missionItem.Index == 0) continue;
                Mission?.AddOrUpdatePoint(missionItem.TransformMissionItemToPointModel());
            }
        }
        
        
        private async Task OpenBrowserImpl(CancellationToken arg)
        {
            var dialog = new ContentDialog
            {
                Title = RS.PlanningPageViewModel_MissionBrowserDialog_Title,
                PrimaryButtonText = RS.PlanningPageViewModel_MissionBrowserDialog_PrimaryButton,
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = RS.PlanningPageViewModel_MissionBrowserDialog_SecondaryButton
            };
            
            using var viewModel = new PlaningMissionBrowserViewModel(_svc, _log);
            viewModel.ApplyDialog(dialog);
            dialog.Content = viewModel;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                OpenMission(viewModel.DialogResult);
            }
        }

        [Reactive]
        public PlaningMissionViewModel? Mission { get; set; }
        
        public void OpenMission(Guid id)
        {
            Mission?.Dispose();
            
            using var handle = _svc.MissionStore.OpenFile(id);
            
            Mission = new PlaningMissionViewModel(handle.Id, handle.Name, _log, _taskFactory, 
                _container, _svc, this);
            
            Mission.Load(handle.File);
            
            _anchorProvider.Update(Mission);
        
            if (Mission.Points.Count > 0)
                Center = Mission.Points.FirstOrDefault()!.MissionAnchor.Location;

            Mission.IsChanged = false;
        }

        public double UploadProgress { get; set; }
        
        [Reactive]
        public PlanningPageViewModelConfig PlanningConfig { get; set; }
    }
}

public interface IPlaningMissionContext : IMap
{
    PlaningMissionViewModel Mission { get; set; }
    void OpenMission(Guid id);
    public double UploadProgress { get; }
}