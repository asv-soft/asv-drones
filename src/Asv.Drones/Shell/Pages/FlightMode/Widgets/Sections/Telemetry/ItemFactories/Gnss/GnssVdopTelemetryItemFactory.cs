using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class GnssVdopTelemetryItemFactory : ITelemetryItemFactory
{
    public const string Id = "gnss-vdop";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IGnssClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var vdopObservable = device
            .GetRequiredMicroservice<IGnssClientEx>()
            .Main.Info.Select(info => info.Vdop ?? double.NaN)
            .Prepend(double.NaN)
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<double>(Id, vdopObservable, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.GnssTelemetry_Vdop_Header,
            ShortHeader = RS.GnssTelemetry_Vdop_Header_Short,
            Icon = MaterialIconKind.GpsFixed,
        };

        static void Update(TelemetryViewModel<double> t, double changes)
        {
            t.Text = changes.ToString("F2");
        }
    }
}
