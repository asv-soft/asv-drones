using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using R3;

namespace Asv.Drones;

public class MotorsSetupPageExtension : IExtensionFor<ISetupPage>
{
    public const string StaticId = "ext.setup.motors";

    string ISupportId<string>.Id => StaticId;

    public void Extend(ISetupPage context, CompositeDisposable contextDispose)
    {
        context
            .Target.Where(w => w is not null)
            .Subscribe(wrapper =>
            {
                var client = wrapper?.Device.GetMicroservice<IMotorTestClient>();

                if (
                    client is null
                    || context.Nodes.Any(node => node.Id.TypeId == SetupMotorsViewModel.PageId)
                )
                {
                    return;
                }

                context.Nodes.Add(
                    new TreePageMenuItem(
                        SetupMotorsViewModel.PageId,
                        RS.SetupMotorsViewModel_Name,
                        SetupMotorsViewModel.Icon,
                        new NavId(SetupMotorsViewModel.PageId),
                        NavId.Empty
                    ).DisposeItWith(contextDispose)
                );
            })
            .DisposeItWith(contextDispose);
    }
}
