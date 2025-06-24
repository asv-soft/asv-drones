using Asv.Avalonia;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Api;

public class MavParamButtonViewModel : MavParamViewModel
{
    public MavParamButtonViewModel()
        : this(
            new MavParamInfo(
                new MavParamTypeMetadata(
                    "A"
                        + NavigationId
                            .GenerateRandomAsString(15)
                            .Replace('.', '_')
                            .Replace('-', '_'),
                    MavParamType.MavParamTypeInt32
                )
                {
                    Units = "MHz",
                    RebootRequired = false,
                    Volatile = false,
                    MinValue = new MavParamValue(-100),
                    ShortDesc = "Test param",
                    LongDesc = "Long description for test param",
                    Group = "System",
                    Category = "System",
                    MaxValue = new MavParamValue(100),
                    DefaultValue = new MavParamValue(50),
                    Increment = new MavParamValue(1),
                }
            ),
            new Subject<MavParamValue>(),
            (_, _) => ValueTask.FromResult(new MavParamValue(100)),
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MavParamButtonViewModel(
        MavParamInfo info,
        Observable<MavParamValue> update,
        InitialReadParamDelegate initReadCallback,
        ILoggerFactory loggerFactory
    )
        : base(info, update, initReadCallback, loggerFactory) { }

    public void InternalWrite()
    {
        Value.Value = 0;
        Value.Value = 1;
        Write();
    }
}
