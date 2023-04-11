using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using DynamicData;

namespace Asv.Drones.Gui.Gbs;

[Export(FlightPageViewModel.UriString, typeof(IViewModelProvider<IMapAnchor>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class FlightGbsMapLayerProvider : IViewModelProvider<IMapAnchor>
{
    [ImportingConstructor]
    public FlightGbsMapLayerProvider(IMavlinkGbsService devices, ILocalizationService loc)
    {
        Items = devices.BaseStations.Transform(_ => new GbsAnchor(_, loc)).ChangeKey((k, _) => _.Id).Transform(_ => (IMapAnchor)_);
    }
    public IObservable<IChangeSet<IMapAnchor, Uri>> Items { get; }
}