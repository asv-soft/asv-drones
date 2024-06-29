using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Avalonia.Map;
using Asv.Common;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api
{
    public class MapPageViewModel : ShellPage, IMap
    {
        private readonly ReadOnlyObservableCollection<IMapAnchor> _markers;
        private readonly ReadOnlyObservableCollection<IMapWidget> _leftWidgets;
        private readonly ReadOnlyObservableCollection<IMapWidget> _rightWidgets;
        private readonly ReadOnlyObservableCollection<IMapWidget> _bottomWidgets;
        private readonly ReadOnlyObservableCollection<IMapAction> _mapActions;
        private IDisposable _disposableMapUpdate;
        private readonly SourceCache<IMapAnchor, Uri> _additionalAnchorsSource;
        private readonly double _baseSizeAnchor = 32;
        
        const double SmallSizeFactor = 0.5;
        const double MediumSizeFactor = 1.1;
        const double LargeSizeFactor = 1.2;
        const double SmallStrokeFactor = 2.0 / 3.0;
        const double LargeStrokeFactor = 7.0 / 3.0;


        /// <summary>
        /// This constructor is used for design time
        /// </summary>
        public MapPageViewModel() : base(WellKnownUri.Undefined)
        {
        }

        public MapPageViewModel(Uri id, IMapService map,
            IEnumerable<IViewModelProvider<IMapStatusItem>> statusItems,
            IEnumerable<IViewModelProvider<IMapMenuItem>> headerMenuItems,
            IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            IEnumerable<IViewModelProvider<IMapWidget>> widgetsSource,
            IEnumerable<IViewModelProvider<IMapAction>> actions) : base(id)
        {
            Disposable.AddAction(() =>
            {
                markers.ForEach(x => x.Dispose());
                widgetsSource.ForEach(x => x.Dispose());
                statusItems.ForEach(x => x.Dispose());
                headerMenuItems.ForEach(x => x.Dispose());
                actions.ForEach(x => x.Dispose());
            });

            #region Map provider

            map.CurrentMapProvider.Subscribe(x => MapProvider = x).DisposeItWith(Disposable);
            this.WhenValueChanged(x => x.MapProvider, false).Subscribe(map.CurrentMapProvider)
                .DisposeItWith(Disposable);

            #endregion

            #region Map anchors

            _additionalAnchorsSource = new SourceCache<IMapAnchor, Uri>(x => x.Id).DisposeItWith(Disposable);
            markers.Select(p => p.Items)
                .Append(_additionalAnchorsSource.Connect())
                .IgnoreNulls()
                .MergeChangeSets()
                .Transform(a => a.Init(this))
                .DisposeMany()
                .Bind(out _markers)
                .Subscribe()
                .DisposeItWith(Disposable);

            this.WhenValueChanged(x => x.IsInAnchorEditMode)
                .Subscribe(x =>
                {
                    foreach (var marker in _markers)
                    {
                        if (marker.IsEditable) marker.IsInEditMode = x;
                    }
                })
                .DisposeItWith(Disposable);

            this.WhenValueChanged(x => x.Zoom).Subscribe(x =>
            {
                foreach (var marker in _markers)
                {
                    marker.Size = x switch
                    {
                        <= 5 => marker.BaseSize * SmallSizeFactor,
                        var zoom and > 5 and <= 10 => marker.BaseSize * (zoom / 10),
                        <= 12 and > 10 => marker.BaseSize * MediumSizeFactor,
                        _ => marker.BaseSize * LargeSizeFactor
                    };
                }
            }).DisposeItWith(Disposable);

            
            #endregion

            #region Widgets

            widgetsSource.Select(p => p.Items)
                .IgnoreNulls()
                .MergeChangeSets()
                .Transform(w => w.Init(this))
                .SortBy(w => w.Order)
                .DisposeMany()
                .Bind(out var widgets)
                .Subscribe()
                .DisposeItWith(Disposable);

            widgets
                .ToObservableChangeSet()
                .Filter(w => w.Location == WidgetLocation.Left)
                .AutoRefresh(w => w.Location)
                .Bind(out _leftWidgets)
                .Subscribe()
                .DisposeItWith(Disposable);
            widgets
                .ToObservableChangeSet()
                .Filter(w => w.Location == WidgetLocation.Right)
                .AutoRefresh(w => w.Location)
                .Bind(out _rightWidgets)
                .Subscribe()
                .DisposeItWith(Disposable);
            widgets
                .ToObservableChangeSet()
                .Filter(w => w.Location == WidgetLocation.Bottom)
                .AutoRefresh(w => w.Location)
                .Bind(out _bottomWidgets)
                .Subscribe()
                .DisposeItWith(Disposable);
            
            #endregion

            #region MapActions

            actions.Select(p => p.Items)
                .IgnoreNulls()
                .MergeChangeSets()
                .Transform(a => a.Init(this))
                .SortBy(a => a.Order)
                .Bind(out _mapActions)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);

            #endregion

            #region Menu & status items

            headerMenuItems.Select(x => x.Items)
                .MergeChangeSets()
                .Transform(x => x.Init(this))
                .DisposeMany()
                .PopulateInto(HeaderItemsSource)
                .DisposeItWith(Disposable);

            statusItems.Select(x => x.Items)
                .MergeChangeSets()
                .Transform(x => x.Init(this))
                .DisposeMany()
                .PopulateInto(StatusItemsSource)
                .DisposeItWith(Disposable);

            #endregion

            this.WhenValueChanged(m => m.ItemToFollow, false)
                .Subscribe(SetUpFollow)
                .DisposeItWith(Disposable);

            /*Markers.ToObservableChangeSet()
                .Transform(_ => (IMenuItem)new MenuItem(_.Id)
                {
                    Header = _.Id.AbsoluteUri,
                    Icon = _.Icon,
                    Command = ReactiveCommand.Create(() =>
                    {
                        Center = _.Location;
                        //SelectedItem = _;
                    })
                })
                .Bind(out var anchorMenuItems)
                .Subscribe()
                .DisposeItWith(Disposable);

            var menuItem = exportedMenuItems.FirstOrDefault(_ => _ is MenuItem);

            if (menuItem != null)
            {
                menuItem.Items = anchorMenuItems;
            }

             Disposable.AddAction(() =>
             {
                 if (menuItem != null)
                     menuItem.Items = null;
                 _disposableMapUpdate?.Dispose();
             });*/

            DeselectAnchorsCommand = ReactiveCommand.Create(() =>
            {
                SelectedItem = null;

                foreach (var mapAnchor in Markers)
                {
                    if (mapAnchor.IsSelected)
                        mapAnchor.IsSelected = false;
                }
            });
        }

        private void SetUpFollow(IMapAnchor? anchor)
        {
            _disposableMapUpdate?.Dispose();
            _disposableMapUpdate = anchor?.WhenAnyValue(_ => _.Location)
                .Subscribe(_ => Center = _);
        }

        public ReadOnlyObservableCollection<IMapAnchor> Markers => _markers;
        public ReadOnlyObservableCollection<IMapWidget> LeftWidgets => _leftWidgets;
        public ReadOnlyObservableCollection<IMapWidget> RightWidgets => _rightWidgets;
        public ReadOnlyObservableCollection<IMapWidget> BottomWidgets => _bottomWidgets;
        public ReadOnlyObservableCollection<IMapAction> MapActions => _mapActions;
        public ICommand DeselectAnchorsCommand { get; set; }

        #region Map properties

        [Reactive] public GMapProvider MapProvider { get; set; } = EmptyProvider.Instance;

        [Reactive] public int MaxZoom { get; set; } = 20;
        [Reactive] public int MinZoom { get; set; } = 1;
        [Reactive] public double Zoom { get; set; } = 10;
        [Reactive] public GeoPoint Center { get; set; }
        [Reactive] public IMapAnchor SelectedItem { get; set; }
        [Reactive] public IMapAnchor ItemToFollow { get; set; }
        [Reactive] public bool IsInAnchorEditMode { get; set; }

        #endregion

        #region Map dialog

        [Reactive] public GeoPoint DialogTarget { get; set; }
        [Reactive] public bool IsInDialogMode { get; set; }
        [Reactive] public string DialogText { get; set; }

        public async Task<GeoPoint> ShowTargetDialog(string text, CancellationToken cancel)
        {
            var tcs = new TaskCompletionSource<GeoPoint>();
            DialogText = text;
            IsInDialogMode = true;

            await using var c1 = cancel.Register(() =>
            {
                tcs.TrySetCanceled();
                IsInDialogMode = false;
                SelectedItem = null;
            });

            this.WhenAnyValue(_ => _.IsInDialogMode).Where(_ => IsInDialogMode == false).Subscribe(_ =>
            {
                if (!tcs.Task.IsCanceled)
                {
                    tcs.TrySetResult(DialogTarget);
                }
            }, cancel);

            await tcs.Task;
            return DialogTarget;
        }

        public ISourceCache<IMapAnchor, Uri> AdditionalAnchorsSource => _additionalAnchorsSource;

        #endregion
    }
}