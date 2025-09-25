using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public abstract class SetupSubpage : TreeSubpage<ISetupPage>, ISetupSubpage
{
    protected SetupSubpage(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Target = new ReactiveProperty<DeviceWrapper?>(null).DisposeItWith(Disposable);
    }

    public override ValueTask Init(ISetupPage context)
    {
        Target.Value = context.Target.CurrentValue;

        return ValueTask.CompletedTask;
    }

    protected ReactiveProperty<DeviceWrapper?> Target { get; }
}
