using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(UdpPortViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class UdpPortView : ReactiveUserControl<UdpPortViewModel>
    {
        public UdpPortView()
        {
            InitializeComponent();
        }
    }
}
