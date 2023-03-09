using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public abstract class MapWidgetBase : ViewModelBase, IMapWidget
    {
        protected MapWidgetBase(Uri id) : base(id)
        {
            
        }
        [Reactive]
        public WidgetLocation Location { get;set; }
        [Reactive]
        public string Title { get;set; }
        [Reactive]
        public MaterialIconKind Icon { get; set; }
        public IMapWidget Init(IMap map)
        {
            Map = map;
            InternalAfterMapInit(map);
            return this;
        }

        protected abstract void InternalAfterMapInit(IMap map);

        protected IMap Map { get; private set; }
    }
}