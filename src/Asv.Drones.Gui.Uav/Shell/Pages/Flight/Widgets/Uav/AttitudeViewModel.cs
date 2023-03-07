using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Vehicle;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav
{
    public class AttitudeViewModel:ViewModelBase
    {
        private readonly IVehicle _vehicle;

        public AttitudeViewModel():base(new Uri("designTime://attitude"))
        {
            
        }

        public AttitudeViewModel(IVehicle vehicle, Uri id):base(id)
        {
            _vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            _vehicle.Roll
                .DistinctUntilChanged()
                .Subscribe(_ => Roll = Math.Round(-_ % 360))
                .DisposeWith(Disposable);
            _vehicle.Pitch
                .Select(_=>Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Pitch = _)
                .DisposeWith(Disposable);
            _vehicle.Yaw.DistinctUntilChanged().Subscribe(_ =>
            {
                var heading = Math.Round(_ % 360);
                if (heading < 0) heading = 360 + heading;
                Heading = heading;
            }).DisposeWith(Disposable);
            _vehicle.AltitudeAboveHome
                .Select(_=>Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Altitude = _)
                .DisposeWith(Disposable);
            _vehicle
                .GpsGroundVelocity
                .Select(_ => Math.Round(_))
                .DistinctUntilChanged()
                .Subscribe(_ => Velocity = _)
                .DisposeWith(Disposable);
            _vehicle.GlobalPosition
                .DistinctUntilChanged()
                .Subscribe(_ => UpdateHome(_, _vehicle.Home.Value))
                .DisposeWith(Disposable);
            _vehicle
                .IsArmed
                .DistinctUntilChanged()
                .Subscribe(_ =>
            {
                IsArmed = _;
                UpdateStatusText(_ ? "Armed" : "Disarmed");
            }).DisposeWith(Disposable);
            _vehicle.ArmedTime
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => ArmedTime = _)
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