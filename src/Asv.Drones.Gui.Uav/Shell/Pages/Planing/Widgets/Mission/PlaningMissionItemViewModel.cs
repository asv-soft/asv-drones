using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class PlaningMissionItemViewModel:ViewModelBase
    {
        private readonly MissionItem _item;
        private readonly PlaningMissionViewModel _parent;
        private IMap _map;

        /// <summary>
        /// This constructor is used by design time tools
        /// </summary>
        public PlaningMissionItemViewModel():base(new Uri($"test{Guid.NewGuid()}"))
        {
            
        }
        public PlaningMissionItemViewModel(Uri baseUri, MissionItem item,
            PlaningMissionViewModel parent) : base(new Uri(baseUri,$"/{item.Index}"))
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _parent = parent;
            Index = _item.Index;
            item.Command.Subscribe(_ => Title = _.GetTitle());
        }
        
        public MissionItem Item => _item;
        public int Index { get; internal set; }
        [Reactive]
        public string Title { get; internal set; }

        public void NavigateMapToMe()
        {
            _parent.NavigateMapToMe(this);
        }
    }

    
}