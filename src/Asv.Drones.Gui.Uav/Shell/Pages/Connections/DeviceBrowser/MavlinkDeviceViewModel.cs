using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class MavlinkDeviceViewModel : ViewModelBase
    {
        private uint _rate;
        private DateTime _lastUpdate;
        private uint _lastRate;

        public MavlinkDeviceViewModel():base(new Uri(DeviceBrowserViewModel.BaseUri,"item"))
        {
            if (Design.IsDesignMode)
            {
                Icon = MaterialIconKind.Quadcopter;
                Name = "Quadcopter [1:1]";
                Description = "Type:Quadcopter, System ID:1, Component ID:{1}, Mavlink version:3";
                BaseModeText = "Mode:UnInit";
                CustomModeText = "10";
                SystemStatusText = "Ready";
                RateText = "0.5 Hz";
                DeviceFullId = UInt16.MinValue;
            }
        }

        public MavlinkDeviceViewModel(IMavlinkDevice info) : base(new Uri(DeviceBrowserViewModel.BaseUri, $"{info.FullId}"))
        {
            DeviceFullId = info.FullId;
            Icon = MavlinkIconHelper.GetIcon(info.Type);
            Name = $"{MavlinkIconHelper.GetTypeName(info.Type):G} [{info.SystemId}:{info.ComponentId}]";
            Description = $"Type: {info.Type.ToString("G").Replace("MavType","")}, System ID: {info.SystemId}, Component ID: {info.ComponentId}, Mavlink Version: {info.MavlinkVersion}";

            Observable.Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3)).Subscribe(_ =>
            {
                var now = DateTime.Now;
                var rate = (((double)_rate - _lastRate) / (now - _lastUpdate).TotalSeconds);
                RateText = $"{rate:F1} Hz";
                _lastUpdate = now;
                _lastRate = _rate;
            }).DisposeItWith(Disposable);

            info.Ping.Sample(TimeSpan.FromSeconds(2)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                ToggleLinkPing = false;
                ToggleLinkPing = true;
            }).DisposeItWith(Disposable);

            info.Ping.Subscribe(_=>Interlocked.Increment(ref _rate)).DisposeItWith(Disposable);

            info.BaseMode.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => BaseModeText = $"Mode: {_.ToString("F").Replace("MavModeFlag","")}").DisposeItWith(Disposable);
            info.SystemStatus.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => SystemStatusText = _.ToString("G").Replace("MavState","")).DisposeItWith(Disposable);
            info.CustomMode.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ => CustomModeText = _.ToString()).DisposeItWith(Disposable);
        }

        


        public ushort DeviceFullId { get; }

        public MaterialIconKind Icon { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        [Reactive]
        public bool ToggleLinkPing { get; set; }

        [Reactive]
        public string BaseModeText { get; set; }
        [Reactive]
        public string CustomModeText { get; set; }
        [Reactive]
        public string SystemStatusText { get; set; }
        [Reactive]
        public string RateText { get; set; }
        
    }
}