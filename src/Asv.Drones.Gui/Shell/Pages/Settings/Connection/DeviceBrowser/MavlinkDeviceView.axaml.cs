using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(MavlinkDeviceViewModel))]
    public partial class MavlinkDeviceView : ReactiveUserControl<MavlinkDeviceViewModel>
    {
        public MavlinkDeviceView()
        {
            InitializeComponent();
        }
    }
}