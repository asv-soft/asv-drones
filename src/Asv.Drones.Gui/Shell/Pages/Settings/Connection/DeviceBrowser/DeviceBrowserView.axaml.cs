using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(DeviceBrowserViewModel))]
    public partial class DeviceBrowserView : ReactiveUserControl<DeviceBrowserViewModel>
    {
        public DeviceBrowserView()
        {
            InitializeComponent();
        }
    }
}