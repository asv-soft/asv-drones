using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones;

public class DefaultSetupExtension(
    [FromKeyedServices(SetupPageViewModel.PageId)] IEnumerable<ITreePage> items
) : IExtensionFor<ISetupPage>
{
    public void Extend(ISetupPage context, CompositeDisposable contextDispose)
    {
        foreach (var treePage in items)
        {
            context.Nodes.Add(treePage);
            treePage.DisposeItWith(contextDispose);
        }
    }
}
