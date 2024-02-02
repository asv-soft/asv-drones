using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(PlaningPageViewModel.UriString, typeof(IViewModelProvider<IMapWidget>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PlaningMissionItemEditorWidgetProvider : ViewModelProviderBase<IMapWidget>
{
    [ImportingConstructor]
    public PlaningMissionItemEditorWidgetProvider(IPlaningMission svc, ILocalizationService loc)
    {
        Source.AddOrUpdate(new PlaningMissionItemEditorViewModel(svc, loc));
    }
}