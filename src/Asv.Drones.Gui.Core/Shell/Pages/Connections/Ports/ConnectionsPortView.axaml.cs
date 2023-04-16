using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
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
