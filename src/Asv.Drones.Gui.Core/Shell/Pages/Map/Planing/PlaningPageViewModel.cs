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
    /// <summary>
    /// Represents the configuration for the PlanningPageViewModel.
    /// </summary>
    public class PlanningPageViewModelConfig
    {
        /// <summary>
        /// Gets or sets the zoom level.
        /// </summary>
        /// <value>
        /// The zoom level as a double.
        /// </value>
        public double Zoom { get; set; }

        /// <summary>
        /// Gets or sets the center point on the map.
        /// </summary>
        /// <value>
        /// The center point on the map.
        /// </value>
        public GeoPoint MapCenter { get; set; }
    }

    /// <summary>
    /// Represents the view model for the Planning page.
    /// </summary>
    [ExportShellPage(UriString)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlaningPageViewModel : MapPageViewModel, IPlaningMissionContext
    {
        /// <summary>
        /// Represents the constant string for the URI.
        /// </summary>
        public const string UriString = "asv:shell.page.map.planing";

        /// <summary>
        /// Represents a constant Uri.
        /// </summary>
        public static readonly Uri Uri = new(UriString);
        private readonly IPlaningMissionAnchorProvider _anchorProvider;

        /// <summary>
        /// Represents an instance of a <see cref="IPlaningMissionPointFactory"/> that is used to create planning mission points.
        /// </summary>
        private readonly IPlaningMissionPointFactory _taskFactory;

        /// <summary>
        /// Represents an instance of the IMavlinkDevicesService interface used for managing Mavlink devices.
        /// </summary>
        private readonly IMavlinkDevicesService _devices;

        /// <summary>
        /// The CompositionContainer used for dependency injection and managing the lifetime of objects.
        /// </summary>
        private readonly CompositionContainer _container;

        /// <summary>
        /// Represents an instance of the Planning Mission service.
        /// </summary>
        private readonly IPlaningMission _svc;

        /// <summary>
        /// Represents a private, read-only instance of the <see cref="ILogService"/> interface.
        /// </summary>
        private readonly ILogService _log;

        /// <summary>
        /// Represents a collection of menu items for uploading.
        /// </summary>
        private ReadOnlyObservableCollection<IHeaderMenuItem> _uploadMenuItems;

        /// <summary>
        /// Holds the collection of download menu items.
        /// </summary>
        private ReadOnlyObservableCollection<IHeaderMenuItem> _downloadMenuItems;

        /// <summary>
        /// Represents a private readonly variable that stores configuration information.
        /// </summary>
        /// <remarks>
        /// This variable is of type IConfiguration and is used to access and utilize configuration information
        /// within the scope of its class. It is marked as readonly, indicating that its value cannot be modified
        /// after initialization.
        /// </remarks>
        private readonly IConfiguration _cfg;

        /// <summary>
        /// View model for the Planning page.
        /// </summary>
        /// <remarks>
        /// This view model provides functionality for uploading and downloading mission files, opening a browser, saving and deleting missions, and updating anchors.
        /// It inherits from the base MapViewModel class.
        /// </remarks>
        /// <param name="map">The map service.</param>
        /// <param name="cfg">The configuration service.</param>
        /// <param name="svc">The planning mission service.</param>
        /// <param name="log">The log service.</param>
        /// <param name="container">The composition container.</param>
        /// <param name="taskFactory">The planning mission point factory.</param>
        /// <param name="devices">The MAVLink devices service.</param>
        /// <param name="exportedMenuItems">A collection of exported menu items.</param>
        /// <param name="markers">A collection of map markers.</param>
        /// <param name="widgets">A collection of map widgets.</param>
        /// <param name="actions">A collection of map actions.</param>
        /// <seealso cref="MapViewModel"/>
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

            _cfg = cfg;
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

        /// <summary>
        /// Uploads a mission to a vehicle using the provided points.
        /// </summary>
        /// <param name="vehicle">The vehicle client to upload the mission to.</param>
        /// <param name="sdr">The SDR client device used for creating vehicle items.</param>
        /// <param name="points">The collection of planning mission points.</param>
        /// <param name="cancel">The cancellation token for cancelling the upload.</param>
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

        /// <summary>
        /// Downloads the mission from the specified vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle client used to download the mission.</param>
        /// <param name="cancel">The cancellation token used to cancel the download process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Tries to close the current mission.
        /// </summary>
        /// <returns>Returns a task representing the asynchronous operation. The task result is a boolean value indicating whether the mission was successfully closed or not.</returns>
        public override async Task<bool> TryClose()
        {
            if (Mission == null) return true;
            
            if (Mission.IsChanged)
            {
                var dialog = new ContentDialog
                {
                    Title = RS.PlaningPageViewModel_DataLossDialog_Title,
                    Content = RS.PlaningPageViewModel_DataLossDialog_Content,
                    PrimaryButtonText = RS.PlaningPageViewModel_DataLossDialog_PrimaryButtonText,
                    SecondaryButtonText = RS.PlaningPageViewModel_DataLossDialog_SecondaryButtonText,
                    CloseButtonText = RS.PlaningPageViewModel_DataLossDialog_CloseButtonText,
                    IsSecondaryButtonEnabled = true
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    Mission.SaveImpl();
                    
                    return true;
                }

                if (result == ContentDialogResult.Secondary) return true;

                if (result == ContentDialogResult.None) return false;
            }

            return true;
        }

        /// <summary>
        /// Opens the browser dialog for selecting a mission.
        /// </summary>
        /// <param name="arg">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
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

        /// <summary>
        /// Gets or sets the planning mission for the property.
        /// </summary>
        /// <remarks>
        /// This property represents the planning mission associated with the property. The planning mission
        /// contains information related to the mission such as the mission's view model.
        /// </remarks>
        /// <seealso cref="PlaningMissionViewModel"/>
        [Reactive]
        public PlaningMissionViewModel? Mission { get; set; }

        /// <summary>
        /// Opens a mission with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the mission to open.</param>
        public void OpenMission(Guid id)
        {
            try
            {
                Mission?.Dispose();

                using (var handle = _svc.MissionStore.OpenFile(id))
                {
                    Mission = new PlaningMissionViewModel(handle.Id, handle.Name, _log, _taskFactory,
                        _container, _svc, this);

                    Mission.Load(handle.File);

                    _anchorProvider.Update(Mission);

                    if (Mission.Points.Count > 0)
                        Center = Mission.Points.FirstOrDefault()!.MissionAnchor.Location;

                    Mission.IsChanged = false;
                }
            }
            catch (Exception e)
            {
                _log.Error("Planing", e.Message, e);   
            }
        }

        /// <summary>
        /// Gets or sets the upload progress in a decimal format.
        /// </summary>
        /// <remarks>
        /// A value between 0 and 1 is expected.
        /// 0 indicates no progress,
        /// while 1 indicates the upload is complete.
        /// </remarks>
        public double UploadProgress { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the PlanningPageViewModel.
        /// The PlanningConfig property is decorated with the [Reactive] attribute, indicating that it is a reactive property.
        /// </summary>
        /// <value>The planning configuration.</value>
        [Reactive]
        public PlanningPageViewModelConfig PlanningConfig { get; set; }
    }
}

/// <summary>
/// Represents the context for planning a mission.
/// </summary>
public interface IPlaningMissionContext : IMap
{
    /// <summary>
    /// Gets or sets the Mission property of type PlaningMissionViewModel.
    /// This property represents the mission associated with the planning process.
    /// </summary>
    /// <value>
    /// The PlaningMissionViewModel object representing the mission.
    /// </value>
    PlaningMissionViewModel Mission { get; set; }

    /// <summary>
    /// Opens the mission specified by the given id.
    /// </summary>
    /// <param name="id">The unique identifier of the mission to be opened.</param>
    void OpenMission(Guid id);

    /// <summary>
    /// Gets the progress of the upload as a decimal value between 0 and 1.
    /// </summary>
    /// <value>
    /// The upload progress as a decimal value.
    /// </value>
    public double UploadProgress { get; }
}