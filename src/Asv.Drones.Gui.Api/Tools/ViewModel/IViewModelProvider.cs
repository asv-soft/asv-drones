using Asv.Common;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Api
{
    public interface IViewModelProvider<TView> : IDisposable
    {
        IObservable<IChangeSet<TView, Uri>> Items { get; }
    }

    public abstract class ViewModelProviderBase<TView> : DisposableOnceWithCancel, IViewModelProvider<TView>
        where TView : IViewModel
    {
        private readonly SourceCache<TView, Uri> _sourceCache;

        protected ViewModelProviderBase()
        {
            _sourceCache = new SourceCache<TView, Uri>(model => model.Id)
                .DisposeItWith(Disposable);
        }

        protected ISourceCache<TView, Uri> Source => _sourceCache;
        public virtual IObservable<IChangeSet<TView, Uri>> Items => Source.Connect().DisposeMany();
    }


    public interface IViewModel : IReactiveObject, IDisposable
    {
        Uri Id { get; }
    }

    public class ViewModelBase : DisposableReactiveObject, IViewModel
    {
        protected ViewModelBase(Uri id)
        {
            Id = id;
        }

        protected ViewModelBase(string id) : this(new Uri(id))
        {
        }

        public Uri Id { get; }
    }

    public class ViewModelBaseWithValidation : DisposableReactiveObjectWithValidation, IViewModel
    {
        protected ViewModelBaseWithValidation(Uri id)
        {
            Id = id;
        }

        protected ViewModelBaseWithValidation(string id) : this(new Uri(id))
        {
        }

        public Uri Id { get; }
    }
}