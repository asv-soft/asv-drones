using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.Vehicle;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui
{
    public class AttitudeViewModel : ViewModelBase
    {
        private readonly IVehicleClient _vehicle;
        private readonly ILocalizationService _localization;

        public AttitudeViewModel() : base(WellKnownUri.UndefinedUri)
        {
            DesignTime.ThrowIfNotDesignMode();
        }

        public AttitudeViewModel(IVehicleClient vehicle, Uri id, ILocalizationService localization) : base(id)
        {
            _localization = localization;

            _vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            _vehicle.Position.Roll
                .DistinctUntilChanged()
                .Subscribe(_ => Roll = Math.Round(-_ % 360))
                .DisposeItWith(Disposable);
            _vehicle.Position.Pitch
                .Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Pitch = _)
                .DisposeItWith(Disposable);
            _vehicle.Position.Yaw.DistinctUntilChanged().Subscribe(_ =>
            {
                var heading = Math.Round(_ % 360);
                if (heading < 0) heading = 360 + heading;
                Heading = heading;
            }).DisposeItWith(Disposable);
            _vehicle.Position.AltitudeAboveHome
                .Select(_localization.Altitude.ConvertFromSi)
                .Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Altitude = _)
                .DisposeItWith(Disposable);
            _vehicle
                .Gnss.Main.GroundVelocity
                .Select(_localization.Velocity.ConvertFromSi)
                .Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Velocity = _)
                .DisposeItWith(Disposable);
            _vehicle.Position.Current
                .Where(_ => _vehicle.Position.Home.Value.HasValue)
                .DistinctUntilChanged()
                .Subscribe(_ => UpdateHome(_, _vehicle.Position.Home.Value))
                .DisposeItWith(Disposable);
            _vehicle
                .Position
                .IsArmed
                .DistinctUntilChanged()
                .Subscribe(_ =>
                {
                    IsArmed = _;
                    UpdateStatusText(_
                        ? RS.AttitudeViewModel_Vehicle_UpdateStatusText_Armed
                        : RS.AttitudeViewModel_Vehicle_UpdateStatusText_Disarmed);
                }).DisposeItWith(Disposable);
            _vehicle.Position.ArmedTime
                .DistinctUntilChanged()
                .Subscribe(_ => ArmedTime = _)
                .DisposeItWith(Disposable);

            vehicle.Connection
                .Where(_ => _.SystemId == vehicle.Identity.TargetSystemId &&
                            _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_ => VibrationX = 100 - _.Payload.VibrationX)
                .DisposeItWith(Disposable);

            vehicle.Connection
                .Where(_ => _.SystemId == vehicle.Identity.TargetSystemId &&
                            _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_ => VibrationY = 100 - _.Payload.VibrationY)
                .DisposeItWith(Disposable);

            vehicle.Connection
                .Where(_ => _.SystemId == vehicle.Identity.TargetSystemId &&
                            _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_ => VibrationZ = 100 - _.Payload.VibrationZ)
                .DisposeItWith(Disposable);

            vehicle.Connection
                .Where(_ => _.SystemId == vehicle.Identity.TargetSystemId &&
                            _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_ => Clipping0 = _.Payload.Clipping0)
                .DisposeItWith(Disposable);

            vehicle.Connection
                .Where(_ => _.SystemId == vehicle.Identity.TargetSystemId &&
                            _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_ => Clipping1 = _.Payload.Clipping1)
                .DisposeItWith(Disposable);

            vehicle.Connection
                .Where(_ => _.SystemId == vehicle.Identity.TargetSystemId &&
                            _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_ => Clipping2 = _.Payload.Clipping2)
                .DisposeItWith(Disposable);
        }

        private void UpdateStatusText(string text)
        {
            StatusText = text;
            Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(_ => { StatusText = ""; }).DisposeItWith(Disposable);
        }

        private void UpdateHome(GeoPoint position, GeoPoint? homePosition)
        {
            if (!homePosition.HasValue)
            {
                HomeAzimuth = null;
            }
            else
            {
                HomeAzimuth = position.Azimuth(homePosition.Value);
            }
        }

        [Reactive] public float VibrationX { get; set; }

        [Reactive] public float VibrationY { get; set; }

        [Reactive] public float VibrationZ { get; set; }

        [Reactive] public uint Clipping0 { get; set; }

        [Reactive] public uint Clipping1 { get; set; }

        [Reactive] public uint Clipping2 { get; set; }

        [Reactive] public double Roll { get; set; }

        [Reactive] public double Pitch { get; set; }

        [Reactive] public double Velocity { get; set; }

        [Reactive] public double Altitude { get; set; }

        [Reactive] public double Heading { get; set; }

        [Reactive] public double? HomeAzimuth { get; set; }

        [Reactive] public string StatusText { get; set; }

        [Reactive] public string MissionDistance { get; set; }

        [Reactive] public bool IsArmed { get; set; }

        [Reactive] public TimeSpan ArmedTime { get; set; }
    }
}