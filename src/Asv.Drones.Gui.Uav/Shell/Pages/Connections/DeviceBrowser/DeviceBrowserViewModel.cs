using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [Export(typeof(IConnectionPart))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DeviceBrowserViewModel : ViewModelBase,IConnectionPart
    {
        public const string BaseUriString = $"{ConnectionsViewModel.BaseUriString}.devices";
        public static readonly Uri BaseUri = new(BaseUriString);

        private readonly ReadOnlyObservableCollection<MavlinkDeviceViewModel> _items;

        public DeviceBrowserViewModel() : base(BaseUri)
        {
            if (Design.IsDesignMode)
            {
                var portList = new List<MavlinkDeviceViewModel>
                {
                    new() { },
                    new() { },
                    new() { },
                };
                _items = new ReadOnlyObservableCollection<MavlinkDeviceViewModel>(new ObservableCollection<MavlinkDeviceViewModel>(portList));



            }
        }

        [ImportingConstructor]
        public DeviceBrowserViewModel(IMavlinkDevicesService svc):this()
        {
            svc.Devices
                .Do(_ => { })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(_=>new MavlinkDeviceViewModel(_))
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            Disposable.AddAction(() => { });
        }
        
        public bool IsRebootRequired { get; private set; }
        
        public int Order => 255;

        public ReadOnlyObservableCollection<MavlinkDeviceViewModel> Items => _items;
    }
}