using Asv.Avalonia;

namespace Asv.Drones;

internal static class DeviceTelemetryDesignPreview
{
    public const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    private static bool _isConfigured;

    public static IUnitService UnitService
    {
        get
        {
            ConfigureUnits();
            return NullUnitService.Instance;
        }
    }

    public static IUnit Unit(string id) => UnitService.Units[id];

    private static void ConfigureUnits()
    {
        if (_isConfigured)
        {
            return;
        }

        var unitService = NullUnitService.Instance;

        unitService.Extend(
            new VelocityUnit(
                DesignTime.Configuration,
                [new VelocityMetersPerSecondUnitItem(), new VelocityMilesPerHourUnitItem()]
            )
        );
        unitService.Extend(
            new ProgressUnit(
                DesignTime.Configuration,
                [new ProgressPercentUnitItem(), new ProgressInPartsUnitItem()]
            )
        );
        unitService.Extend(
            new CapacityUnit(DesignTime.Configuration, [new CapacityMilliAmperePerHourUnitItem()])
        );
        unitService.Extend(
            new AmperageUnit(
                DesignTime.Configuration,
                [new AmperageAmpereUnitItem(), new AmperageMilliAmpereUnitItem()]
            )
        );
        unitService.Extend(
            new VoltageUnit(
                DesignTime.Configuration,
                [new VoltageVoltUnitItem(), new VoltageMilliVoltUnitItem()]
            )
        );

        _isConfigured = true;
    }
}
