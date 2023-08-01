using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
[Export(PlaningPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class AnchorsEditorWidgetProvider : ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public AnchorsEditorWidgetProvider(ILocalizationService loc)
    {
        Source.AddOrUpdate(new AnchorsEditorViewModel(loc));
    }
}