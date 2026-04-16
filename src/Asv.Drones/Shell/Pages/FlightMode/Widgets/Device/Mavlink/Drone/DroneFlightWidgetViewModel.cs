using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Drones;

public class DroneFlightWidgetViewModel
    : FlightWidgetViewModel<MavlinkClientDevice, IDroneFlightWidget>,
        IDroneFlightWidget
{
    public const string WidgetId = "drone";

    private readonly IDeviceManager _deviceManager;
    private int _order;

    public DroneFlightWidgetViewModel()
        : this(
            NullDeviceManager.Instance,
            NullLoggerFactory.Instance,
            NullExtensionService.Instance
        ) { }

    public DroneFlightWidgetViewModel(
        IDeviceManager deviceManager,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(WidgetId, loggerFactory, ext)
    {
        _deviceManager = deviceManager;
        Position = WorkspaceDock.Left;
    }

    public override int Order => _order;

    public MavlinkClientDevice? Device { get; private set; }

    public override void InitWith(MavlinkClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var mavlinkId =
            device.Id as MavlinkClientDeviceId
            ?? throw new Exception($"Should be {typeof(MavlinkClientDeviceId)}");
        Header = device.Id.ToString();
        Icon = _deviceManager.GetIcon(device.Id);
        IconColor = _deviceManager.GetDeviceColor(device.Id);
        device
            .Name.ObserveOnUIThreadDispatcher()
            .Subscribe(x => Header = x)
            .DisposeItWith(Disposable);
        Device = device;
        InitArgs(device.Id.AsString());

        _order = CreateOrderFromId(mavlinkId);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var item in SectionsView)
        {
            yield return item;
        }
    }

    protected override void AfterLoadExtensions()
    {
        // nothing to do
    }

    private static int CreateOrderFromId(MavlinkClientDeviceId id)
    {
        return (id.Id.Target.SystemId * 1000) + id.Id.Target.ComponentId;
    }
}
