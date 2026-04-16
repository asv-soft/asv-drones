using System;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class PluginFlightItemWidgetExtension(ILoggerFactory loggerFactory, IExtensionService ext)
    : IExtensionFor<IFlightModePage>
{
    public void Extend(IFlightModePage context, CompositeDisposable contextDispose)
    {
#if RELEASE
        return;
#endif
        var vm = new PluginFlightItemViewModel(loggerFactory, ext);

        vm.Header = "Plugin payload for something";

        context.Widgets.Add(vm);

        Task.Run(async () =>
        {
            while (!contextDispose.IsDisposed)
            {
                if (context.IsDisposed)
                {
                    return;
                }

                await Task.Delay(5000);
                context.Widgets.Remove(vm);
                await Task.Delay(5000);
                context.Widgets.Add(vm);
            }
        });
    }
}
