using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class GnssModeTelemetryItemFactory : ITelemetryItemFactory
{
    public const string Id = "gnss-mode";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var gnssModeObservable = device
            .GetRequiredMicroservice<IGnssClientEx>()
            .Main.Info.Select(info => info.FixType)
            .Prepend(Mavlink.Common.GpsFixType.GpsFixTypeNoGps)
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<Mavlink.Common.GpsFixType>(Id, gnssModeObservable, Update)
        {
            Density = TileDensity.Regular,
            Header = RS.GnssTelemetry_Mode_Header,
            ShortHeader = RS.GnssTelemetry_Mode_Header_Short,
            Icon = MaterialIconKind.GpsFixed,
        };

        static void Update(
            TelemetryViewModel<Mavlink.Common.GpsFixType> t,
            Mavlink.Common.GpsFixType changes
        )
        {
            t.Text = changes.GetShortDisplayName();
        }
    }
}
