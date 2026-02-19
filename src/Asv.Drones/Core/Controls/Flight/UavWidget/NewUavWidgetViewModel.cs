// TODO: asv-soft-u08

using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Material.Icons;
using R3;

namespace Asv.Drones;

public class NewUavWidgetViewModel : MapWidget, IUavFlightWidget
{
    private const string WidgetId = "widget-uav";
    private const AsvColorKind DefaultStatusColor = AsvColorKind.Info5;

    public NewUavWidgetViewModel()
        : base(WidgetId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        
        Icon = MaterialIconKind.AccountFile;
        IconColor = AsvColorKind.Info5;
        
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
        unitService.Extend(
            new AngleUnit(
                DesignTime.Configuration,
                [new AngleDegreeUnitItem()]
            )
        );

        var altitudeUnit = unitService.Units[AltitudeUnit.Id];
        var capacityUnit = unitService.Units[CapacityUnit.Id];
        var amperageUnit = unitService.Units[AmperageUnit.Id];
        var voltageUnit = unitService.Units[VoltageUnit.Id];
        var progressUnit = unitService.Units[ProgressUnit.Id];
        var angleUnit = unitService.Units[AngleUnit.Id];
        
        var altitudeAgl = new ReactiveProperty<double>(10).DisposeItWith(Disposable);
        var altitudeMsl = new ReactiveProperty<double>(14).DisposeItWith(Disposable);
        var heading = new ReactiveProperty<double>(29).DisposeItWith(Disposable);
        var azimuth = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var homeAzimuth = new ReactiveProperty<double>(30).DisposeItWith(Disposable);
        var velocity = new ReactiveProperty<double>(199).DisposeItWith(Disposable);
        var batteryAmperage = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var batteryVoltage = new ReactiveProperty<double>(34).DisposeItWith(Disposable);
        var batteryCharge = new ReactiveProperty<double>(123).DisposeItWith(Disposable);
        var batteryConsumed = new ReactiveProperty<double>(39).DisposeItWith(Disposable);
        var roll = new ReactiveProperty<double>(10).DisposeItWith(Disposable);
        var pitch = new ReactiveProperty<double>(30).DisposeItWith(Disposable);
        
        AltitudeUavIndicator = new AltitudeUavIndicatorViewModel(
                nameof(AltitudeUavIndicator),
            DesignTime.LoggerFactory, 
            altitudeAgl,
            altitudeMsl,
            altitudeUnit.CurrentUnitItem,
            DefaultStatusColor)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        
        BatteryUavIndicator = new BatteryUavIndicatorViewModel(
                nameof(BatteryUavIndicator),
            DesignTime.LoggerFactory, 
            batteryCharge, 
            batteryAmperage, 
            batteryVoltage, 
            batteryConsumed,
            progressUnit.CurrentUnitItem,
            amperageUnit.CurrentUnitItem,
            capacityUnit.CurrentUnitItem,
            voltageUnit.CurrentUnitItem,
            DefaultStatusColor)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        VelocityUavIndicator = new VelocityUavIndicatorViewModel(
                nameof(VelocityUavIndicator),
            DesignTime.LoggerFactory,
            unitService,
            velocity,
            DefaultStatusColor)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AngleUavRttIndicator = new AngleUavRttIndicatorViewModel(
            nameof(AngleUavRttIndicator),
            DesignTime.LoggerFactory,
            pitch,
            roll,
            angleUnit.CurrentUnitItem,
            DefaultStatusColor
            )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }
    
    public AltitudeUavIndicatorViewModel AltitudeUavIndicator { get; }
    public BatteryUavIndicatorViewModel BatteryUavIndicator { get; }
    public VelocityUavIndicatorViewModel VelocityUavIndicator { get; }
    public AngleUavRttIndicatorViewModel AngleUavRttIndicator { get; }
    
    public IClientDevice Device => null!;

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return AltitudeUavIndicator;
        yield return BatteryUavIndicator;
        yield return VelocityUavIndicator;
        yield return AngleUavRttIndicator;
    }
}