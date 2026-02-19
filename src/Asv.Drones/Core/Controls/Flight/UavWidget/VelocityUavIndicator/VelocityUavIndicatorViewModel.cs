// TODO: asv-soft-u08

using Asv.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class VelocityUavIndicatorViewModel : SplitDigitRttBoxViewModel
{
    public VelocityUavIndicatorViewModel(
        NavigationId id, 
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        ReactiveProperty<double> velocity,
        AsvColorKind defaultStatusColor)
        : base(
            id,
            loggerFactory,
            unitService,
            VelocityUnit.Id,
            velocity,
            null
        )
    {
        Header = RS.UavRttItem_Velocity;
        ShortHeader = "GS";
        Icon = MaterialIconKind.Speedometer;
        Status = defaultStatusColor;
        FormatString = "F2";
    }
}