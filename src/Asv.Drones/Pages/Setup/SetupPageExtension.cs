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
            .Target.SubscribeAwait(
                async (wrapper, ct) =>
                {
                    await TryAddSetupFrameTypeSubpage(context, wrapper, contextDispose, ct);
                }
            )
            .DisposeItWith(contextDispose);
    }

    private async ValueTask TryAddSetupFrameTypeSubpage(
        ISetupPage context,
        DeviceWrapper? wrapper,
        CompositeDisposable contextDispose,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var paramsClient = wrapper?.Device.GetMicroservice<IParamsClient>();
        if (paramsClient is null)
        {
            return;
        }

        var param = await paramsClient.Read("FRAME_TYPE", ct);

        // TODO: make read return nullable result
        if (param is null)
        {
            return;
        }

        if (context.Nodes.Any(node => node.Id == SetupFrameTypeViewModel.PageId))
        {
            return;
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
    }
}
