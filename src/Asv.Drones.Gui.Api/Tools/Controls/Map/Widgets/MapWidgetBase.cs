using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api
{
    public abstract class MapWidgetBase : ViewModelBaseWithValidation, IMapWidget
    {
        protected MapWidgetBase(Uri id) : base(id)
        {
        }

        protected MapWidgetBase(string id) : base(id)
        {
        }

        [Reactive] public WidgetLocation Location { get; set; }
        [Reactive] public string Title { get; set; }
        public int Order { get; set; }
        [Reactive] public MaterialIconKind Icon { get; set; }

        public IMapWidget Init(IMap context)
        {
            Map = context;
            InternalAfterMapInit(context);
            return this;
        }

        protected abstract void InternalAfterMapInit(IMap context);

        public IMap Map { get; private set; }
    }
}