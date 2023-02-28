using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [ExportShellPage(WellKnownUri.ShellPageFlight)]
    [PartCreationPolicy(CreationPolicy.Shared)] //Important shared mode
    public class FlightViewModel: ViewModelBase,IShellPage,IMap
    {
        private ReadOnlyObservableCollection<IMapAnchor> _markers;

        public FlightViewModel() : base(new(WellKnownUri.ShellPageFlight))
        {
            if (Design.IsDesignMode)
            {

            }
        }

        [ImportingConstructor]
        public FlightViewModel(
            [ImportMany] IEnumerable<IViewModelProvider<IMapAnchor>> markers,
            IMapService map,
            INavigationService navigation):this()
        {
            map.CurrentMapProvider.Subscribe(_ => MapProvider = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.MapProvider).Subscribe(map.CurrentMapProvider).DisposeItWith(Disposable);

            markers.Select(_ => _.Items)
                .IgnoreNulls()
                .Merge()
                .Transform(_ => _.Init(this))
                .ObserveOn(RxApp.MainThreadScheduler)
                .DisposeMany()
                .Bind(out _markers)
                .Subscribe()
                .DisposeItWith(Disposable);
        }



        [Reactive] 
        public GMapProvider MapProvider { get; set; } = EmptyProvider.Instance;

        [Reactive] public int MaxZoom { get; set; } = 20;
        [Reactive] public int MinZoom { get; set; } = 1;
        [Reactive] public double Zoom { get; set; } = 7;
        [Reactive]
        public GeoPoint Center { get; set; }

        public ReadOnlyObservableCollection<IMapAnchor> Markers => _markers;
        [Reactive]
        public IMapAnchor SelectedItem { get; set; }

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

        public void SetArgs(Uri link)
        {
            
        }
    }
}