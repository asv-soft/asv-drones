using System.Linq;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class FrameTypeSetupPageExtension(ILoggerFactory loggerFactory) : IExtensionFor<ISetupPage>
{
    public void Extend(ISetupPage context, CompositeDisposable contextDispose)
    {
        context
            .Target.Where(w => w is not null)
            .Subscribe(wrapper =>
            {
                if (wrapper is null)
                {
                    return;
                }

                var frameClient = wrapper.Value.Device.GetMicroservice<IFrameClient>();

                if (
                    frameClient is null
                    || context.Nodes.Any(node => node.Id == SetupFrameTypeViewModel.PageId)
                )
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
            })
            .DisposeItWith(contextDispose);
    }
}
