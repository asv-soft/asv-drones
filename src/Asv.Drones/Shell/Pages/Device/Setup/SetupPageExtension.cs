using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;
using CompositeDisposable = R3.CompositeDisposable;

namespace Asv.Drones;

[ExportExtensionFor<ISetupPage>]
[method: ImportingConstructor]
public class SetupPageExtension(ILoggerFactory loggerFactory) : IExtensionFor<ISetupPage>
{
    public void Extend(ISetupPage context, CompositeDisposable contextDispose)
    {
        context
            .Target.Where(w => w is not null)
            .SubscribeAwait(
                async (wrapper, ct) =>
                {
                    if (wrapper is null)
                    {
                        return;
                    }

                    await TryAddSetupFrameTypeSubpage(context, wrapper.Value, contextDispose, ct);
                },
                AwaitOperation.Drop
            )
            .DisposeItWith(contextDispose);
    }

    private ValueTask TryAddSetupFrameTypeSubpage(
        ISetupPage context,
        DeviceWrapper wrapper,
        CompositeDisposable contextDispose,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var frameClient = wrapper.Device.GetMicroservice<IFrameClient>();

        if (
            frameClient is null
            || context.Nodes.Any(node => node.Id == SetupFrameTypeViewModel.PageId)
        )
        {
            return ValueTask.CompletedTask;
        }

        context.Nodes.Add(
            new TreePage(
                SetupFrameTypeViewModel.PageId,
                RS.SetupFrameTypeViewModel_Name,
                MaterialIconKind.ThemeLightDark,
                SetupFrameTypeViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );

        return ValueTask.CompletedTask;
    }
}
