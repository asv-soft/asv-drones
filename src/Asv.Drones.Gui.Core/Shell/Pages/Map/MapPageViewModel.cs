using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class MapPageViewModel:ViewModelBase,IShellPage,IMap
    {
        private readonly ReadOnlyObservableCollection<IMapAnchor> _markers;
        private readonly ReadOnlyObservableCollection<IMapWidget> _widgets;
        private readonly ReadOnlyObservableCollection<IMapWidget> _leftWidgets;
        private readonly ReadOnlyObservableCollection<IMapWidget> _rightWidgets;
        private readonly ReadOnlyObservableCollection<IMapWidget> _bottomWidgets;
        
        /// <summary>
        /// This constructor is used for design time
        /// </summary>
        public MapPageViewModel():base(new Uri(WellKnownUri.Shell + Guid.NewGuid()))
        {
            
        }

        public MapPageViewModel(Uri id, IMapService map, 
            IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            IEnumerable<IViewModelProvider<IMapWidget>> widgets):base(id)
        {
            
            #region Map provider

            map.CurrentMapProvider.Subscribe(_ => MapProvider = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.MapProvider).Subscribe(map.CurrentMapProvider).DisposeItWith(Disposable);

            #endregion
            
            #region Map anchors

            markers.Select(_ => _.Items)
                .IgnoreNulls()
                .Merge()
                .Transform(_ => _.Init(this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .DisposeMany()
                .Bind(out _markers)
                .Subscribe()
                .DisposeItWith(Disposable);

            #endregion
           
            #region Widgets
            
            widgets.Select(_ => _.Items)
                .IgnoreNulls()
                .Merge()
                .Transform(_ => _.Init(this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .DisposeMany()
                .Bind(out _widgets)
                .Subscribe()
                .DisposeItWith(Disposable);

            _widgets
                .ToObservableChangeSet()
                .Filter(_ => _.Location == WidgetLocation.Left)
                .AutoRefresh(_ => _.Location)
                .Bind(out _leftWidgets)
                .Subscribe()
                .DisposeItWith(Disposable);
            _widgets
                .ToObservableChangeSet()
                .Filter(_ => _.Location == WidgetLocation.Right)
                .AutoRefresh(_ => _.Location)
                .Bind(out _rightWidgets)
                .Subscribe()
                .DisposeItWith(Disposable);
            _widgets
                .ToObservableChangeSet()
                .Filter(_ => _.Location == WidgetLocation.Bottom)
                .AutoRefresh(_ => _.Location)
                .Bind(out _bottomWidgets)
                .Subscribe()
                .DisposeItWith(Disposable);
            
            #endregion
        }

        public ReadOnlyObservableCollection<IMapAnchor> Markers => _markers;
        public ReadOnlyObservableCollection<IMapWidget> LeftWidgets => _leftWidgets;
        public ReadOnlyObservableCollection<IMapWidget> RightWidgets => _rightWidgets;
        public ReadOnlyObservableCollection<IMapWidget> BottomWidgets => _bottomWidgets;
        
        #region Map properties

        [Reactive] 
        public GMapProvider MapProvider { get; set; } = EmptyProvider.Instance;

        [Reactive] public int MaxZoom { get; set; } = 20;
        [Reactive] public int MinZoom { get; set; } = 1;
        [Reactive] public double Zoom { get; set; } = 7;
        [Reactive]
        public GeoPoint Center { get; set; }
        [Reactive]
        public IMapAnchor SelectedItem { get; set; }

        #endregion

        #region Map dialog

        [Reactive]
        public GeoPoint DialogTarget { get; set; }
        [Reactive]
        public bool IsInDialogMode { get; set; }
        [Reactive]
        public string DialogText { get; set; }

        public async Task<GeoPoint> ShowTargetDialog(string text, CancellationToken cancel)
        {
            DialogText = text;
            IsInDialogMode = true;
            var tcs = new TaskCompletionSource();
            await using var c1 = cancel.Register(() => tcs.TrySetCanceled());
            this.WhenAnyValue(_ => _.IsInDialogMode).Where(_ => IsInDialogMode == false).Subscribe(_ => tcs.TrySetResult(), cancel);
            await tcs.Task;
            return DialogTarget;
        }

        #endregion

        public virtual void SetArgs(Uri link)
        {
            
        }
    }
}