using System;
using System.Linq;
using System.Reactive.Linq;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetFlightControlSectionExtension(IServiceProvider services)
    : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var flightControlViewModel =
            services.GetRequiredKeyedService<FlightControlSectionViewModel>(
                FlightControlSectionViewModel.WidgetId
            );
        context.Sections.Add(flightControlViewModel);

        flightControlViewModel.InitWith(context.Device ?? throw new NullReferenceException());
    }
}
