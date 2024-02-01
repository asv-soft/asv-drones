using Asv.Common;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents a provider for view models of a specific type.
    /// </summary>
    /// <typeparam name="TView">The type of the view model.</typeparam>
    public interface IViewModelProvider<TView>:IDisposable
    {
        /// <summary>
        /// Gets an observable collection of <see cref="IChangeSet{TView, Uri}"/> for the items.
        /// </summary>
        /// <typeparam name="TView">The type of the items in the collection.</typeparam>
        /// <returns>An observable collection of changes made to the items.</returns>
        IObservable<IChangeSet<TView, Uri>> Items { get; }
    }

    /// <summary>
    /// Represents a base class for view model providers.
    /// </summary>
    /// <typeparam name="TView">The type of the view model.</typeparam>
    public abstract class ViewModelProviderBase<TView> : DisposableOnceWithCancel, IViewModelProvider<TView>
        where TView : IViewModel
    {
        /// <summary>
        /// Private readonly variable _sourceCache is an instance of the generic class SourceCache<TView, Uri>. This variable is used to cache sources of type TView based on their corresponding
        /// URIs.
        /// </summary>
        /// <typeparam name="TView">The type of the views that are being cached.</typeparam>
        /// <typeparam name="Uri">The type of the URIs that are being used as keys to cache the views.</typeparam>
        private readonly SourceCache<TView, Uri> _sourceCache;

        /// <summary>
        /// Protected default constructor for the ViewModelProviderBase class.
        /// Initializes the _sourceCache member with a new instance of SourceCache<TView, Uri> and assigns it to _sourceCache.
        /// </summary>
        protected ViewModelProviderBase()
        {
            _sourceCache = new SourceCache<TView, Uri>(model => model.Id)
                .DisposeItWith(Disposable);
        }

        /// <summary>
        /// Gets the protected property Source.
        /// </summary>
        /// <typeparam name="TView">The type of the view.</typeparam>
        /// <returns>The <see cref="ISourceCache{TView, Uri}"/> object.</returns>
        protected ISourceCache<TView, Uri> Source => _sourceCache;

        /// <summary>
        /// Gets the observable sequence of items with their associated URIs.
        /// </summary>
        /// <typeparam name="TView">The type of the view objects.</typeparam>
        /// <returns>An IObservable<IChangeSet<TView, Uri>> representing the sequence of changes to the items.</returns>
        public virtual IObservable<IChangeSet<TView, Uri>> Items => Source.Connect().DisposeMany();
    }


    /// <summary>
    /// Represents a view model that implements the <see cref="IReactiveObject"/> and <see cref="IDisposable"/> interfaces.
    /// </summary>
    public interface IViewModel:IReactiveObject, IDisposable
    {
        /// <summary>
        /// Gets the unique identifier for the property.
        /// </summary>
        /// <value>
        /// The unique identifier for the property.
        /// </value>
        Uri Id { get; }
    }

    /// <summary>
    /// Base class for view models.
    /// </summary>
    public class ViewModelBase : DisposableReactiveObject, IViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected ViewModelBase(Uri id)
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the ViewModelBase class with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier for the ViewModel.</param>
        protected ViewModelBase(string id): this(new Uri(id))
        {
            
        }

        /// <summary>
        /// Gets the unique identifier of the property.
        /// </summary>
        /// <value>
        /// The unique identifier of the property.
        /// </value>
        public Uri Id { get; }
    }

    /// <summary>
    /// Represents a base view model class with validation.
    /// </summary>
    public class ViewModelBaseWithValidation : DisposableReactiveObjectWithValidation, IViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBaseWithValidation"/> class with the specified ID.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="ViewModelBaseWithValidation"/>.</param>
        protected ViewModelBaseWithValidation(Uri id)
        {
            Id = id;
        }

        /// Initializes a new instance of the ViewModelBaseWithValidation class with the specified id.
        /// @param id The ID of the ViewModelBaseWithValidation.
        /// /
        protected ViewModelBaseWithValidation(string id): this(new Uri(id))
        {
            
        }

        /// <summary>
        /// Gets the unique identifier for the property.
        /// </summary>
        /// <value>
        /// A <see cref="Uri"/> representing the ID of the property.
        /// </value>
        public Uri Id { get; }
    }


}