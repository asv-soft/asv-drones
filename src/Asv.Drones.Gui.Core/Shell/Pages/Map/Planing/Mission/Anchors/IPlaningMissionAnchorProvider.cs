using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

/// <summary>
/// Provides functionality to update a planning mission anchor.
/// </summary>
public interface IPlaningMissionAnchorProvider
{
    /// <summary>Updates the specified planning mission</summary>
    /// <param name="mission">The PlanningMissionViewModel object containing the updated information. The parameter can be null.</param>
    /// <remarks>
    /// This method updates the planning mission with the provided information. If the parameter 'mission' is null, no update is performed.
    /// </remarks>
    void Update(PlaningMissionViewModel? mission);
}

/// <summary>
/// Provides the anchor for planning missions on the map.
/// </summary>
[Export(PlaningPageViewModel.UriString, typeof(IViewModelProvider<IMapAnchor>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PlaningMissionAnchorProvider : ViewModelProviderBase<IMapAnchor>, IPlaningMissionAnchorProvider
{
    /// <summary>
    /// Represents a disposable object used for populating a mission subject.
    /// </summary>
    private IDisposable? _missionPopulateSubject;

    /// <summary>
    /// This class provides anchor data for planning missions.
    /// </summary>
    [ImportingConstructor]
    public PlaningMissionAnchorProvider()
    {
        
        Disposable.AddAction(() =>
        {
            _missionPopulateSubject?.Dispose();
            _missionPopulateSubject = null;
        });
        
        
    }

    /// <summary>
    /// Updates the planning mission with the provided mission view model.
    /// </summary>
    /// <param name="mission">The mission view model to update.</param>
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