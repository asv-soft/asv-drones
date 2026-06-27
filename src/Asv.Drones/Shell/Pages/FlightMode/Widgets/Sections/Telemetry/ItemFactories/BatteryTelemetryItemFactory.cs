using System.Text;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;
using TelemetryHelper = Asv.Drones.Api.TelemetryHelper;

namespace Asv.Drones;

public sealed class BatteryTelemetryItemFactory(IUnitService unitService) : ITelemetryItemFactory
{
    public const string Id = "battery";

    public string ItemId => Id;

    public bool CanCreate(in IClientDevice device) =>
        device.GetMicroservice<IPositionClientEx>() is not null
        && device.GetMicroservice<ITelemetryClientEx>() is not null;

    public ITileViewModel Create(in IClientDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var positionClientEx = device.GetRequiredMicroservice<IPositionClientEx>();
        var telemetryClient = device.GetRequiredMicroservice<ITelemetryClientEx>();

        var batteryObservable = telemetryClient
            .BatteryCharge.Prepend(double.NaN)
            .CombineLatest(
                telemetryClient.BatteryCurrent.Prepend(double.NaN),
                telemetryClient.BatteryVoltage.Prepend(double.NaN),
                unitService.Units[ProgressUnit.Id].CurrentUnitItem,
                unitService.Units[AmperageUnit.Id].CurrentUnitItem,
                unitService.Units[CapacityUnit.Id].CurrentUnitItem,
                unitService.Units[VoltageUnit.Id].CurrentUnitItem,
                (
                    charge,
                    amperage,
                    voltage,
                    progressUnit,
                    amperageUnit,
                    capacityUnit,
                    voltageUnit
                ) =>
                {
                    var consumed = amperage.ApproximatelyEquals(0d)
                        ? double.NaN
                        : Math.Round(
                            amperage * positionClientEx.ArmedTime.CurrentValue.TotalHours,
                            2
                        );

                    return new BatteryTelemetryData(
                        charge,
                        amperage,
                        voltage,
                        consumed,
                        progressUnit,
                        amperageUnit,
                        capacityUnit,
                        voltageUnit
                    );
                }
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200));

        return new TelemetryViewModel<BatteryTelemetryData>(Id, batteryObservable, Update)
        {
            Density = TileDensity.Regular,
            Header = RS.BatteryTelemetry_Header,
            ShortHeader = RS.BatteryTelemetry_Header_Short,
            Icon = MaterialIconKind.Battery80,
        };

        void Update(TelemetryViewModel<BatteryTelemetryData> t, BatteryTelemetryData changes)
        {
            var percentCharge = changes.Charge * 100;
            t.Text = changes.ProgressUnit.PrintFromSi(percentCharge, "F2");
            t.Units = changes.ProgressUnit.Symbol;
            t.Progress = percentCharge;
            t.StatusText = CreateStatusString(changes);
            t.StatusColor = GetStatusColor(changes.Charge);
            t.ProgressColor = GetStatusColor(changes.Charge);
            t.StatusIcon = GetStatusIcon(changes.Charge);
            t.StatusIconColor = GetStatusColor(changes.Charge);
        }
    }

    private static string CreateStatusString(BatteryTelemetryData changes)
    {
        var sb = new StringBuilder();
        sb.Append(changes.VoltageUnit.PrintFromSiWithUnits(changes.Voltage, "F2"))
            .Append('|')
            .Append(changes.AmperageUnit.PrintFromSiWithUnits(changes.Amperage, "F2"))
            .Append('|')
            .Append(changes.CapacityUnit.PrintFromSiWithUnits(changes.Consumed, "F2"));
        return sb.ToString();
    }

    private static AsvColorKind GetStatusColor(double percent)
    {
        return percent switch
        {
            > 0.7d => AsvColorKind.Success,
            > 0.5d => AsvColorKind.Warning,
            > 0.4d => AsvColorKind.Warning | AsvColorKind.Blink,
            < 0.3d => AsvColorKind.Error | AsvColorKind.Blink,
            _ => TelemetryHelper.DefaultStatusColor,
        };
    }

    private static MaterialIconKind GetStatusIcon(double percent)
    {
        return percent switch
        {
            > 0.4d and < 0.7d => MaterialIconKind.AlertCircle,
            < 0.3d => MaterialIconKind.CloseCircle,
            _ => MaterialIconKind.CheckCircle,
        };
    }
}

#pragma warning disable SA1313
public record BatteryTelemetryData(
    double Charge,
    double Amperage,
    double Voltage,
    double Consumed,
    IUnitItem ProgressUnit,
    IUnitItem AmperageUnit,
    IUnitItem CapacityUnit,
    IUnitItem VoltageUnit
);
#pragma warning restore SA1313
