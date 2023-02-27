using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(ConnectionsPortsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ConnectionsPortView : ReactiveUserControl<ConnectionsPortsViewModel>
    {
        public ConnectionsPortView()
        {
            InitializeComponent();
        }
    }
}
