using System;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui
{
    public class MavlinkIdViewModel : TreePageWithValidationViewModel
    {
        public const uint DeviceTimeoutMin = 1; // 1 second
        public const uint DeviceTimeoutMax = 15 * 60; // 15 minute

        public MavlinkIdViewModel() : base(WellKnownUri.ShellPageSettingsConnections)
        {
            this.ValidationRule(_ => _.DeviceTimeout, _ => _ is >= DeviceTimeoutMin and <= DeviceTimeoutMax,
                    _ => string.Format(RS.ConnectionsIdentificationViewModel_DeviceTimeout, DeviceTimeoutMin,
                        DeviceTimeoutMax))
                .DisposeItWith(Disposable);


            if (Design.IsDesignMode)
            {
                SystemId = 254;
                ComponentId = 254;
                SelectedRate = Rates.First();
                DeviceTimeout = 30;
            }
        }

        [ImportingConstructor]
        public MavlinkIdViewModel(IMavlinkDevicesService svc, ISettingsPageContext context) : this()
        {
            svc.NeedReloadToApplyConfig
                .Where(x => x)
                .Subscribe(_ => context.SetRebootRequired())
                .DisposeItWith(Disposable);

            svc.SystemId
                .Subscribe(_ => SystemId = _)
                .DisposeItWith(Disposable);
            this.WhenValueChanged(_ => _.SystemId, false)
                .DistinctUntilChanged()
                .Subscribe(svc.SystemId)
                .DisposeItWith(Disposable);

            svc.ComponentId
                .Subscribe(_ => ComponentId = _)
                .DisposeItWith(Disposable);
            this.WhenValueChanged(_ => _.ComponentId, false)
                .DistinctUntilChanged()
                .Subscribe(svc.ComponentId)
                .DisposeItWith(Disposable);

            svc.HeartbeatRate
                .Subscribe(_ => SelectedRate = new RateItem(_))
                .DisposeItWith(Disposable);
            this.WhenValueChanged(_ => _.SelectedRate, false)
                .DistinctUntilChanged()
                .Subscribe(_ => svc.HeartbeatRate.OnNext(_.Time)).DisposeItWith(Disposable);

            svc.DeviceTimeout
                .Subscribe(_ => DeviceTimeout = (uint)_.TotalSeconds)
                .DisposeItWith(Disposable);
            this.WhenValueChanged(_ => _.DeviceTimeout, false)
                .Where(_ => HasErrors == false)
                .DistinctUntilChanged()
                .Subscribe(_ => svc.DeviceTimeout.OnNext(TimeSpan.FromSeconds(_)))
                .DisposeItWith(Disposable);
        }


        public byte[] Ids { get; } = Enumerable.Range(1, 254).Select(_ => (byte)_).ToArray();

        public RateItem[] Rates { get; } =
        {
            new(TimeSpan.FromMilliseconds(2000)),
            new(TimeSpan.FromMilliseconds(1000)),
            new(TimeSpan.FromMilliseconds(500)),
            new(TimeSpan.FromMilliseconds(200)),
            new(TimeSpan.FromMilliseconds(100)),
        };


        [Reactive] public RateItem SelectedRate { get; set; }

        [Reactive] public byte SystemId { get; set; }

        [Reactive] public byte ComponentId { get; set; }

        [Reactive] public uint DeviceTimeout { get; set; }
    }

    public struct RateItem
    {
        public RateItem(TimeSpan time)
        {
            Time = time;
        }

        public TimeSpan Time { get; }

        public override string ToString()
        {
            return string.Format($"{(1.0 / Time.TotalSeconds):F1}", RS.ConnectionsIdentificationViewModel_ToString);
        }
    }
}