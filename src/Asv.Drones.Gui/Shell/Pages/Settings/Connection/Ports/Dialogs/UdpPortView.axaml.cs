using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(UdpPortViewModel))]
    public partial class UdpPortView : ReactiveUserControl<UdpPortViewModel>
    {
        public UdpPortView()
        {
            InitializeComponent();
        }
    }
}