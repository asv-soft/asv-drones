using DynamicData;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    
    public interface IViewModelProvider<TView>
    {
        IObservable<IChangeSet<TView, Uri>> Items { get; }
    }

    public class ViewModelProviderBase<TView> : IViewModelProvider<TView>
        where TView : IViewModel
    {
        private readonly SourceCache<TView, Uri> _sourceCache = new(_ => _.Id);

        protected ISourceCache<TView, Uri> Source => _sourceCache;

        public IObservable<IChangeSet<TView, Uri>> Items => _sourceCache.Connect();
    }


    public interface IViewModel:IReactiveObject, IDisposable
    {
        Uri Id { get; }
    }

    public class ViewModelBase : DisposableViewModelBase, IViewModel
    {
        public ViewModelBase(Uri id)
        {
            Id = id;
        }

        public Uri Id { get; }
    }


}