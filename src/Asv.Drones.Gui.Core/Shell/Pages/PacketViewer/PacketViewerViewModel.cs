using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PacketViewerViewModel : ViewModelBase, IShellPage
{
    public const string UriString = ShellPage.UriString + ".packetViewer";
    public static readonly Uri Uri = new Uri(UriString);
    
    public PacketViewerViewModel() : base(Uri)
    {
    }
    
    [ImportingConstructor]
    public PacketViewerViewModel(IMavlinkDevicesService mavlinkDevicesService) : this()
    {
        
    }

    public void SetArgs(Uri link)
    {
    }
}