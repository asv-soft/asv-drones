using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

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
[ExportShellPage(WellKnownUri.ShellPageMapPlaning)]
public class PlaningPageViewModel : MapPageViewModel, IPlaningMissionContext
{
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
    /// Represents an instance of the Planning Mission service.
    /// </summary>
    private readonly IPlaningMission _svc;

    /// <summary>
    /// Represents a private, read-only instance of the <see cref="ILogService"/> interface.
    /// </summary>
    private readonly ILogService _log;

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
    [ImportingConstructor]
    public PlaningPageViewModel(IMapService map, IConfiguration cfg, IPlaningMission svc, ILogService log,
        IApplicationHost container, IPlaningMissionPointFactory taskFactory, IMavlinkDevicesService devices,
        [ImportMany(WellKnownUri.ShellPageMapPlaning)]
        IEnumerable<IViewModelProvider<IMapMenuItem>> headerMenuItems,
        [ImportMany(WellKnownUri.ShellPageMapPlaning)]
        IEnumerable<IViewModelProvider<IMapStatusItem>> statusItems,
        [ImportMany(WellKnownUri.ShellPageMapPlaning)]
        IEnumerable<IViewModelProvider<IMapAnchor>> markers,
        [ImportMany(WellKnownUri.ShellPageMapPlaning)]
        IEnumerable<IViewModelProvider<IMapWidget>> widgets,
        [ImportMany(WellKnownUri.ShellPageMapPlaning)]
        IEnumerable<IViewModelProvider<IMapAction>> actions)
        : base(WellKnownUri.ShellPageMapPlaningUri, map, statusItems, headerMenuItems, markers, widgets, actions)
    {
        Title = RS.PlaningShellMenuItem_Name;
        Icon = MaterialIconKind.MapMarkerCheck;
        PlanningConfig = cfg.Get<PlanningPageViewModelConfig>();

        _cfg = cfg;
        _svc = svc;
        _log = log;
        _devices = devices;
        _taskFactory = taskFactory;

        HeaderItemsSource.AddOrUpdate(
            new HeaderPlaningFileMenuItem
            {
                Items = new ReadOnlyObservableCollection<IMenuItem>(new ObservableCollection<IMenuItem>(new IMenuItem[]
                {
                    new HeaderPlaningFileOpenMenuItem
                    {
                        Command = ReactiveCommand.CreateFromTask(OpenBrowserImpl).DisposeItWith(Disposable)
                    },
                    new HeaderPlaningFileSaveMenuItem
                    {
                        Command = ReactiveCommand.Create(() => { Mission.SaveCmd.Execute().Subscribe(); },
                            this.WhenAnyValue(_ => _.Mission).Select(_ => _ != null)).DisposeItWith(Disposable)
                    },
                    new HeaderPlaningFileSaveAsMenuItem
                    {
                        Command = ReactiveCommand.CreateFromTask(OpenSavingBrowserImpl,
                            this.WhenAnyValue(_ => _.Mission).Select(_ => _ != null)).DisposeItWith(Disposable)
                    },
                    new HeaderPlaningFileDeleteMenuItem
                    {
                        Command = ReactiveCommand.Create(() =>
                        {
                            _svc.MissionStore.DeleteFile(Mission.MissionId);
                            Mission = null;
                        }, this.WhenAnyValue(_ => _.Mission).Select(_ => _ != null)).DisposeItWith(Disposable)
                    }
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
            .Subscribe(_ => { _anchorProvider.Update(_.Value); }).DisposeItWith(Disposable);
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
    /// Opens the browser dialog for saving a mission.
    /// </summary>
    /// <param name="arg">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    private async Task OpenSavingBrowserImpl(CancellationToken arg)
    {
        var dialog = new ContentDialog
        {
            Title = RS.PlanningPageViewModel_MissionSavingBrowserDialog_Title,
            PrimaryButtonText = RS.PlanningPageViewModel_MissionSavingBrowserDialog_PrimaryButton,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = RS.PlanningPageViewModel_MissionSavingBrowserDialog_SecondaryButton
        };

        using var viewModel = new PlaningMissionSavingBrowserViewModel(_svc, _log, Mission?.Name);
        viewModel.ApplyDialog(dialog);
        dialog.Content = viewModel;
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            viewModel.SaveAsImpl(out var id);
            
            var points = Mission?.Points;

            if (points == null) throw new NullReferenceException();
            OpenMission(id);

            foreach (var point in points) Mission?.AddOrUpdatePoint(point.Point);
            
            Mission?.SaveImpl();
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

    public IMavlinkDevicesService Devices => _devices;

    /// <summary>
    /// Opens a mission with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the mission to open.</param>
    public void OpenMission(Guid id)
    {
        try
        {
            Mission?.Dispose();

            using var handle = _svc.MissionStore.OpenFile(id);
            Mission = new PlaningMissionViewModel(handle.Id, handle.Name, _log,
                _taskFactory, _svc, this);
            Mission.Load(handle.File);

            _anchorProvider.Update(Mission);

            if (Mission.Points.Count > 0)
            {
                foreach (var point in Mission.Points)
                {
                    if (point is { MissionAnchor: not null })
                    {
                        Center = point.MissionAnchor.Location;
                        break;
                    }
                }
            }

            Mission.IsChanged = false;
        }
        catch (Exception? e)
        {
            _log.Error("Planing", e.Message, e);
        }
    }

    /// <summary>
    /// Gets or sets the configuration for the PlanningPageViewModel.
    /// The PlanningConfig property is decorated with the [Reactive] attribute, indicating that it is a reactive property.
    /// </summary>
    /// <value>The planning configuration.</value>
    [Reactive]
    public PlanningPageViewModelConfig PlanningConfig { get; set; }
}

/// <summary>
/// Represents the context for planning a mission.
/// </summary>
public interface IPlaningMissionContext : IMap
{
    public IMavlinkDevicesService Devices { get; }

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
}