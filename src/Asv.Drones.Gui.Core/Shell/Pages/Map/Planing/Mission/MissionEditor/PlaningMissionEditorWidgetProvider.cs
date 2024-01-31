using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(PlaningPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PlaningMissionEditorWidgetProvider : ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public PlaningMissionEditorWidgetProvider(IPlaningMission svc, ILocalizationService loc)
    {
        Source.AddOrUpdate(new PlaningMissionEditorViewModel(svc, loc));
    }
}