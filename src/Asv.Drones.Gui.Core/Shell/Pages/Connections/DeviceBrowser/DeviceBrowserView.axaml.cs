using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
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
