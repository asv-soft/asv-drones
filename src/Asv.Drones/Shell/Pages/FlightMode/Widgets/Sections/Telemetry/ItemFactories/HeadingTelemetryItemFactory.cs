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

public sealed class HeadingTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "heading";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var headingObservable = device
            .GetRequiredMicroservice<IPositionClientEx>()
            .Yaw.Select(Math.Truncate)
            .Prepend(double.NaN)
            .CombineLatest(
                unitService.Units[AngleUnit.Id].CurrentUnitItem,
                (heading, unit) => new HeadingRttBoxData(heading, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<HeadingRttBoxData>(Id, headingObservable, Update)
        {
            Density = TileDensity.Inline,
            Header = RS.HeadingTelemetry_Header,
            ShortHeader = RS.HeadingTelemetry_Header,
            Icon = MaterialIconKind.SunAzimuth,
        };

        static void Update(TelemetryViewModel<HeadingRttBoxData> t, HeadingRttBoxData changes)
        {
            t.Text = changes.AngleUnit.PrintFromSi(changes.Heading, "F0");
            t.Units = changes.AngleUnit.Symbol;
        }
    }
}

#pragma warning disable SA1313
public record HeadingRttBoxData(double Heading, IUnitItem AngleUnit);
#pragma warning restore SA1313
