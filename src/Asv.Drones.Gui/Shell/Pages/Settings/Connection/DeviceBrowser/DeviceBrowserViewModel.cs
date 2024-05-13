using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using DynamicData;

namespace Asv.Drones.Gui
{
    public class DeviceBrowserViewModel : TreePageViewModel
    {
        private readonly ReadOnlyObservableCollection<MavlinkDeviceViewModel> _items;

        public DeviceBrowserViewModel() : base(WellKnownUri.ShellPageSettingsConnectionsDevicesUri)
        {
            DesignTime.ThrowIfNotDesignMode();
            var portList = new List<MavlinkDeviceViewModel>
            {
                new() { },
                new() { },
                new() { },
            };
            _items = new ReadOnlyObservableCollection<MavlinkDeviceViewModel>(
                new ObservableCollection<MavlinkDeviceViewModel>(portList));
        }

        [ImportingConstructor]
        public DeviceBrowserViewModel(IMavlinkDevicesService svc) : base(WellKnownUri
            .ShellPageSettingsConnectionsDevicesUri)
        {
            svc.Devices
                .Do(_ => { })
                .Transform(_ => new MavlinkDeviceViewModel(_))
                .Bind(out _items)
                .DisposeMany()
                .Subscribe()
                .DisposeItWith(Disposable);
            Disposable.AddAction(() => { });
        }

        public ReadOnlyObservableCollection<MavlinkDeviceViewModel> Items => _items;
    }
}