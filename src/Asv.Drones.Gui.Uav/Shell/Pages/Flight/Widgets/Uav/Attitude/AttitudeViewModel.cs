using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Asv.Mavlink.Vehicle;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class AttitudeViewModel:ViewModelBase
    {
        private readonly IVehicleClient _vehicle;
        private readonly ILocalizationService _localization;
        
        public AttitudeViewModel():base(new Uri("designTime://attitude"))
        {
            
        }

        public AttitudeViewModel(IVehicleClient vehicle, Uri id, ILocalizationService localization) : base(id)
        {
            _localization = localization;
            
            _vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            _vehicle.Position.Roll
                .DistinctUntilChanged()
                .Subscribe(_ => Roll = Math.Round(-_ % 360))
                .DisposeWith(Disposable);
            _vehicle.Position.Pitch
                .Select(_=>Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Pitch = _)
                .DisposeWith(Disposable);
            _vehicle.Position.Yaw.DistinctUntilChanged().Subscribe(_ =>
            {
                var heading = Math.Round(_ % 360);
                if (heading < 0) heading = 360 + heading;
                Heading = heading;
            }).DisposeWith(Disposable);
            _vehicle.Position.AltitudeAboveHome
                .Select(_localization.Altitude.ConvertFromSi)
                .Select(_=>Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Altitude = _)
                .DisposeWith(Disposable);
            _vehicle
                .Gnss.Main.GroundVelocity
                .Select(_localization.Velocity.ConvertFromSi)
                .Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Velocity = _)
                .DisposeWith(Disposable);
            _vehicle.Position.Current
                .Where(_=>_vehicle.Position.Home.Value.HasValue)
                .DistinctUntilChanged()
                .Subscribe(_ => UpdateHome(_, _vehicle.Position.Home.Value))
                .DisposeWith(Disposable);
            _vehicle
                .Position
                .IsArmed
                .DistinctUntilChanged()
                .Subscribe(_ =>
            {
                IsArmed = _;
                UpdateStatusText(_ ? "Armed" : "Disarmed"); // TODO: Localize
            }).DisposeWith(Disposable);
            _vehicle.Position.ArmedTime
                .DistinctUntilChanged()
                .Subscribe(_ => ArmedTime = _)
                .DisposeWith(Disposable);
            
            vehicle.Connection
                .Where(_=>_.SystemId == vehicle.Identity.TargetSystemId && _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_=> VibrationX = 1 - _.Payload.VibrationX)
                .DisposeWith(Disposable);

            vehicle.Connection
                .Where(_=>_.SystemId == vehicle.Identity.TargetSystemId && _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                
                .Subscribe(_=> VibrationY = 1 - _.Payload.VibrationY)
                .DisposeWith(Disposable);
            
            vehicle.Connection
                .Where(_=>_.SystemId == vehicle.Identity.TargetSystemId && _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_=> VibrationZ = 1 - _.Payload.VibrationZ)
                .DisposeWith(Disposable);
            
            vehicle.Connection
                .Where(_=>_.SystemId == vehicle.Identity.TargetSystemId && _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_=> Clipping0 = _.Payload.Clipping0)
                .DisposeWith(Disposable);
            
            vehicle.Connection
                .Where(_=>_.SystemId == vehicle.Identity.TargetSystemId && _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_=> Clipping1 = _.Payload.Clipping1)
                .DisposeWith(Disposable);
            
            vehicle.Connection
                .Where(_=>_.SystemId == vehicle.Identity.TargetSystemId && _.MessageId == VibrationPacket.PacketMessageId)
                .Cast<VibrationPacket>()
                .Subscribe(_=> Clipping2 = _.Payload.Clipping2)
                .DisposeWith(Disposable);
        }
        
        private void UpdateStatusText(string text)
        {
            StatusText = text;
            Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(_ =>
            {
                StatusText = "";
            }).DisposeItWith(Disposable);
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
        
        [Reactive]
        public float VibrationX { get; set; }
        
        [Reactive]
        public float VibrationY { get; set; }

        [Reactive]
        public float VibrationZ { get; set; }

        [Reactive]
        public uint Clipping0 { get; set; }
        
        [Reactive]
        public uint Clipping1 { get; set; }

        [Reactive]
        public uint Clipping2 { get; set; }

        [Reactive]
        public double Roll { get; set; }

        [Reactive]
        public double Pitch { get; set; }

        [Reactive]
        public double Velocity { get; set; }

        [Reactive]
        public double Altitude { get; set; }

        [Reactive]
        public double Heading { get; set; }

        [Reactive]
        public double? HomeAzimuth { get; set; }

        [Reactive]
        public string StatusText { get; set; }

        [Reactive]
        public string MissionDistance { get; set; }

        [Reactive]
        public bool IsArmed { get; set; }

        [Reactive]
        public TimeSpan ArmedTime { get; set; }
    }
}