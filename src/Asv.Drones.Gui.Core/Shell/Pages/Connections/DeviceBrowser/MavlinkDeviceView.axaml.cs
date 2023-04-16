using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(MavlinkDeviceViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MavlinkDeviceView : ReactiveUserControl<MavlinkDeviceViewModel>
    {
        public MavlinkDeviceView()
        {
            InitializeComponent();
        }
    }
}
