using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class LoggerWidgetProvider : ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public LoggerWidgetProvider(ILogService log)
    {
        Source.AddOrUpdate(new LoggerViewModel(log));
    }
}