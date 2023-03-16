using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
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
