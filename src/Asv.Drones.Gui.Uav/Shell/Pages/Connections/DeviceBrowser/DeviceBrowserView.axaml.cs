using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(DeviceBrowserViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class DeviceBrowserView : ReactiveUserControl<DeviceBrowserViewModel>
    {
        public DeviceBrowserView()
        {
            InitializeComponent();
        }
    }
}
