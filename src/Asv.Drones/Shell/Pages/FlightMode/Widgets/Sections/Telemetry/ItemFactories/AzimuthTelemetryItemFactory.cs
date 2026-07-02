using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class AzimuthTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "azimuth";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var azimuthObservable = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Yaw.CombineLatest(
                unitService.Units[AngleUnit.Id].CurrentUnitItem,
                (value, unit) => new AzimuthRttBoxData(Math.Round(value, 2), unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<AzimuthRttBoxData>(Id, azimuthObservable, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.AzimuthTelemetry_Header,
            ShortHeader = RS.AzimuthTelemetry_Header,
            Icon = MaterialIconKind.SunAzimuth,
        };

        static void Update(TelemetryViewModel<AzimuthRttBoxData> t, AzimuthRttBoxData changes)
        {
            t.Text = changes.AngleUnit.PrintFromSi(changes.Azimuth, "F2");
            t.Units = changes.AngleUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record AzimuthRttBoxData(double Azimuth, IUnitItem AngleUnit);
#pragma warning restore SA1313
