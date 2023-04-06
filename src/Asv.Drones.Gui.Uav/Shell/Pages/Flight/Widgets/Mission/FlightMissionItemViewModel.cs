using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class FlightMissionItemViewModel : ViewModelBase
{
    private readonly MissionItem _item;
    private readonly FlightUavViewModel _parent;
    private IMap _map;

    public FlightMissionItemViewModel(Uri baseUri, MissionItem item, FlightUavViewModel parent) 
        : base(new Uri(baseUri,$"/flight/{item.Index}"))
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _parent = parent;
        Index = _item.Index;
        item.Command.Subscribe(_ => Title = _.GetTitle());
    }
    
    public MissionItem Item => _item;
    
    [Reactive]
    public int Index { get; internal set; }
    
    [Reactive]
    public string Title { get; internal set; }
}