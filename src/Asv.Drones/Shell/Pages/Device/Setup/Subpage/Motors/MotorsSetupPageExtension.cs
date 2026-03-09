using System.Linq;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class MotorsSetupPageExtension(ILoggerFactory loggerFactory) : IExtensionFor<ISetupPage>
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

                var client = wrapper.Value.Device.GetMicroservice<IMotorTestClient>();

                if (
                    client is null
                    || context.Nodes.Any(node => node.Id == SetupMotorsViewModel.PageId)
                )
                {
                    return;
                }

                context.Nodes.Add(
                    new TreePage(
                        SetupMotorsViewModel.PageId,
                        RS.SetupMotorsViewModel_Name,
                        SetupMotorsViewModel.Icon,
                        SetupMotorsViewModel.PageId,
                        NavigationId.Empty,
                        loggerFactory
                    ).DisposeItWith(contextDispose)
                );
            })
            .DisposeItWith(contextDispose);
    }
}
