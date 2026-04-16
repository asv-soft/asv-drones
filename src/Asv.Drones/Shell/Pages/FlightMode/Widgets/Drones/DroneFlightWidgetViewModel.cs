using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public class DroneFlightWidgetViewModel
    : ExtendableViewModel<IDroneFlightWidget>,
        IDroneFlightWidget
{
    public const string WidgetId = "drone-flight-widget";

    private readonly IDeviceManager _deviceManager;
    private DeviceId? _deviceId;

    public DroneFlightWidgetViewModel(
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(WidgetId, loggerFactory, ext)
    {
        _deviceManager = deviceManager;
        Position = WorkspaceDock.Left;

        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);

        DashboardWidgets = [];
        DashboardWidgets.SetRoutableParent(this).DisposeItWith(Disposable);
        DashboardWidgets.DisposeRemovedItems().DisposeItWith(Disposable);

        DashboardWidgetsView = DashboardWidgets
            .ToNotifyCollectionChangedSlim()
            .DisposeItWith(Disposable);
    }

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public WorkspaceDock Position
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanExpand
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableList<IMenuItem> Menu { get; } = [];

    public MenuTree? MenuView { get; }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public int DisplayPriority { get; private set; }
    public int SubOrder { get; private set; }

    public DeviceId DeviceId =>
        _deviceId
        ?? throw new InvalidOperationException(
            "DeviceId accessed before Attach. Extension executed before widget initialization — "
                + "verify FlightWidgetExtension.TryCreateWidget calls Attach before returning."
        );

    public ObservableList<IDashboardWidget> DashboardWidgets { get; }

    public NotifyCollectionChangedSynchronizedViewList<IDashboardWidget> DashboardWidgetsView { get; }

    public void Attach(DeviceId deviceId)
    {
        if (_deviceId is not null)
        {
            throw new InvalidOperationException("Already attached");
        }

        _deviceId = deviceId;
        var device = _deviceManager.Explorer.Devices[deviceId];

        Header = deviceId.ToString();
        Icon = _deviceManager.GetIcon(deviceId);
        IconColor = _deviceManager.GetDeviceColor(deviceId);
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(deviceId.AsString());

        DisplayPriority = deviceId.DisplayPriority;
        SubOrder = 0;
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var item in DashboardWidgetsView)
        {
            yield return item;
        }
    }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }
}
