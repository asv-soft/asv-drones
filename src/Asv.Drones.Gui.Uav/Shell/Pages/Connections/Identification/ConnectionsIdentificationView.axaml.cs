using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(ConnectionsIdentificationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ConnectionsIdentificationView : ReactiveUserControl<ConnectionsIdentificationViewModel>
    {
        public ConnectionsIdentificationView()
        {
            InitializeComponent();
        }
    }
}
