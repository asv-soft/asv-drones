using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using DynamicData;

namespace Asv.Drones.Gui;

[Export(WellKnownUri.ShellPageSettings, typeof(IViewModelProvider<ITreePageMenuItem>))]
[Shared]
public class DefaultSettingsPageProvider : ViewModelProviderBase<ITreePageMenuItem>
{
    [ImportingConstructor]
    public DefaultSettingsPageProvider(
        [ImportMany(WellKnownUri.ShellPageSettings)] IEnumerable<ITreePageMenuItem> items)
    {
        Source.AddOrUpdate(items);
    }
}