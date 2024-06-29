using System;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui;

public class AdsbAnchor : MapAnchorBase
{
    public AdsbAnchor(IAdsbClientDevice src, IAdsbVehicle device)
        : base($"{WellKnownUri.ShellPageMapFlightAnchor}/{src.FullId}/{device.IcaoAddress}")
    {
        Size = 48;
        BaseSize = 48;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Center;
        StrokeThickness = 1;
        Stroke = Brushes.SeaGreen;
        IconBrush = Brushes.Teal;
        IsVisible = true;
        Icon = MaterialIconKind.Plane;

        // TODO: create device subscriptions
        device.CallSign.Subscribe(_ => Title = _).DisposeItWith(Disposable);
        device.Location.Subscribe(_ => Location = _).DisposeItWith(Disposable);
        device.Heading.Subscribe(_ => RotateAngle = _ - 45).DisposeItWith(Disposable);
    }
}