using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public abstract class SetupSubpage : TreeSubpage<ISetupPage>, ISetupSubpage
{
    protected SetupSubpage(NavigationId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory) { }

    public override ValueTask Init(ISetupPage context)
    {
        context
            .Target.SubscribeAwait(
                async (wrapper, ct) =>
                {
                    if (wrapper is not null)
                    {
                        await OnDeviceConnected(wrapper.Value.Device, ct);
                    }
                }
            )
            .DisposeItWith(Disposable);

        return ValueTask.CompletedTask;
    }

    protected abstract ValueTask OnDeviceConnected(IClientDevice device, CancellationToken cancel);
}
