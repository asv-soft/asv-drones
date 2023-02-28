using Asv.Avalonia.Map;
using Asv.Common;
using DynamicData;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace Asv.Drones.Gui.Core
{
    public interface IMap
    {
        int MaxZoom { get; set; }
        int MinZoom { get; set; }
        double Zoom { get; set; }
        GeoPoint Center { get; set; }
        ReadOnlyObservableCollection<IMapAnchor> Markers { get; }
        IMapAnchor SelectedItem { get; set; }
        Task<GeoPoint> ShowTargetDialog(string text, CancellationToken cancel);
    }

    

    public interface IMapAnchor : IMapAnchorViewModel,IViewModel
    {
        IMapAnchor Init(IMap map);
    }

    public class MapAnchorBase : MapAnchorViewModel, IMapAnchor
    {
        protected readonly CompositeDisposable Disposable = new();
        private readonly Uri _id;

        public MapAnchorBase(Uri id)
        {
            _id = id;
        }

        public IMapAnchor Init(IMap map)
        {
            Map = map;
            InternalWhenMapLoaded(map);
            return this;
        }

        protected IMap Map { get; private set; }

        protected virtual void InternalWhenMapLoaded(IMap map)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public void Dispose()
        {
            Disposable.Dispose();
        }

        public Uri Id => _id;
    }
}