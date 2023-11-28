using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public interface IPlaningMissionAnchorProvider
{
    void Update(PlaningMissionViewModel? mission);
}

[Export(PlaningPageViewModel.UriString, typeof(IViewModelProvider<IMapAnchor>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PlaningMissionAnchorProvider : ViewModelProviderBase<IMapAnchor>, IPlaningMissionAnchorProvider
{
    private IDisposable? _missionPopulateSubject;

    [ImportingConstructor]
    public PlaningMissionAnchorProvider()
    {
        
        Disposable.AddAction(() =>
        {
            _missionPopulateSubject?.Dispose();
            _missionPopulateSubject = null;
        });
        
        
    }

    public void Update(PlaningMissionViewModel? mission)
    {
        _missionPopulateSubject?.Dispose();
        _missionPopulateSubject = null;
        var polygon = new PlanningMissionPathPolygon("planing-mission-polygon");
        Source.Clear();
        Source.AddOrUpdate(polygon);
        if (mission == null) return;
        
        Source.Connect()
            .OnItemAdded(_ =>
            {
                if(_ is not PlaningMissionRoiPointAnchor && _ is PlaningMissionAnchor anchor)
                    polygon.Points.AddOrUpdate(anchor);
            })
            .OnItemRemoved(_ =>
            {
                if(_ is PlaningMissionAnchor anchor)
                    polygon.Points.Remove(anchor);
            })
            .WhenPropertyChanged(_ => _.Location)
            .Subscribe(_ =>
            {
                polygon.Points.AddOrUpdate(Source.Items.Where(_ => _ is not PlaningMissionRoiPointAnchor & _ is PlaningMissionAnchor).Cast<PlaningMissionAnchor>());
            })
            .DisposeItWith(Disposable);
        
        _missionPopulateSubject = mission.Points
            .ToObservableChangeSet(point => point.Id)
            .AutoRefresh(_ => _.Type)
            .AutoRefresh(_ => _.Index)
            // This is because merge many dont delete items from source when task removed
            // See similar problem here => https://www.magentaize.xyz/posts/mergemany-do-not-remove-elements-of-removed-observables-in-dynamicdata/
            .Do(changes =>
            {
                foreach (var x in changes)
                {
                    if (x.Reason == ChangeReason.Remove)
                    {
                        Source.Remove(x.Current.MissionAnchor);
                    }
                }
            })
            .Transform(_ => (IMapAnchor)_.MissionAnchor)
            .DisposeMany()
            .PopulateInto(Source);
    }
}