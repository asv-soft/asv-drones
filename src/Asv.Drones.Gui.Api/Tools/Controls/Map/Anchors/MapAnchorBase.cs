using System.Reactive.Disposables;
using Asv.Avalonia.Map;

namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// Base implementation of <see cref="IMapAnchor"/>
    /// </summary>
    public class MapAnchorBase : MapAnchorViewModel, IMapAnchor
    {
        protected readonly CompositeDisposable Disposable = new();
        private readonly Uri _id;
        private double _disposeFlag;

        public MapAnchorBase(Uri id)
        {
            _id = id;
        }

        public MapAnchorBase(string id)
        {
            _id = new Uri(id);
        }

        public IMapAnchor Init(IMap map)
        {
            Map = map;
            InternalWhenMapLoaded(map);
            return this;
        }

        protected virtual void InternalWhenMapLoaded(IMap map)
        {
            // do nothing            
        }

        protected IMap Map { get; private set; }

        protected virtual void InternalDisposeOnce()
        {
            Disposable.Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposeFlag, 1, 0) != 0) return;
            InternalDisposeOnce();
            GC.SuppressFinalize(this);
        }

        public Uri Id => _id;
    }
}