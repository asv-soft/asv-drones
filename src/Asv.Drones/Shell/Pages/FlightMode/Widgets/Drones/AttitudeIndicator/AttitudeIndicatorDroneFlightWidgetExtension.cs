using System;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class AttitudeIndicatorDroneFlightWidgetExtension(IServiceProvider containerHost)
    : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var dashboard = containerHost.GetKeyedService<IDashboardWidget>(
            AttitudeIndicatorViewModel.WidgetId
        );

        dashboard?.Attach(context.DeviceId!);
        if (dashboard != null)
        {
            context.DashboardWidgets.Add(dashboard);
        }
    }
}
