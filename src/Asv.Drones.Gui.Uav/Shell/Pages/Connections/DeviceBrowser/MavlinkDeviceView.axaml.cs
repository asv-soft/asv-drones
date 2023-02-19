using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
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
