using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class GnssHdopTelemetryItemFactory : ITelemetryItemFactory
{
    public const string Id = "gnss-hdop";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var hdopObservable = device
            .GetRequiredMicroservice<IGnssClientEx>()
            .Main.Info.Select(info => info.Hdop ?? double.NaN)
            .Prepend(double.NaN)
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<double>(Id, hdopObservable, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.GnssTelemetry_Hdop_Header,
            ShortHeader = RS.GnssTelemetry_Hdop_Header_Short,
            Icon = MaterialIconKind.GpsFixed,
        };

        static void Update(TelemetryViewModel<double> t, double changes)
        {
            t.Text = changes.ToString("F2");
        }
    }
}
