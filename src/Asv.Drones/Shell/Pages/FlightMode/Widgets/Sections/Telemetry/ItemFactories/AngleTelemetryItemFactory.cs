using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class AngleTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "angle";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();

        var angleObservable = positionClientEx
            .Pitch.Prepend(double.NaN)
            .CombineLatest(
                positionClientEx.Roll.Prepend(double.NaN),
                unitService.Units[AngleUnit.Id].CurrentUnitItem,
                (pitch, roll, unit) => new AngleRttBoxData(pitch, roll, unit)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<AngleRttBoxData>(Id, angleObservable, Update)
        {
            Density = TileDensity.Regular,
            Header = RS.AngleTelemetry_Header,
            ShortHeader = RS.AngleTelemetry_Header,
            Icon = MaterialIconKind.Altimeter,
        };

        static void Update(TelemetryViewModel<AngleRttBoxData> t, AngleRttBoxData changes)
        {
            t.Text = changes.AngleUnit.PrintFromSi(changes.Pitch, "F2");
            t.Units = changes.AngleUnit.Symbol;
            t.StatusText =
                $"{RS.AngleTelemetry_Roll}: {changes.AngleUnit.PrintFromSiWithUnits(changes.Roll, "F2")}";
        }
    }
}

#pragma warning disable SA1313
public record AngleRttBoxData(double Pitch, double Roll, IUnitItem AngleUnit);
#pragma warning restore SA1313
