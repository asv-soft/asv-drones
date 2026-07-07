using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class FrameTypeSetupPageExtension : IExtensionFor<ISetupPage>
{
    public const string StaticId = "ext.setup.frame-type";

    string ISupportId<string>.Id => StaticId;

    public void Extend(ISetupPage context, CompositeDisposable contextDispose)
    {
        context
            .Target.Where(w => w is not null)
            .Subscribe(wrapper =>
            {
                var frameClient = wrapper?.Device.GetMicroservice<IFrameClient>();

                if (
                    frameClient is null
                    || context.Nodes.Any(node => node.Id.TypeId == SetupFrameTypeViewModel.PageId)
                )
                {
                    return;
                }

                context.Nodes.Add(
                    new TreePageMenuItem(
                        SetupFrameTypeViewModel.PageId,
                        RS.SetupFrameTypeViewModel_Name,
                        SetupFrameTypeViewModel.Icon,
                        new NavId(SetupFrameTypeViewModel.PageId),
                        NavId.Empty
                    ).DisposeItWith(contextDispose)
                );
            })
            .DisposeItWith(contextDispose);
    }
}
