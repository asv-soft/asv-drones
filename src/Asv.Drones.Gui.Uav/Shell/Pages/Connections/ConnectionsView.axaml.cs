using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(ConnectionsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ConnectionsView : ReactiveUserControl<ConnectionsViewModel>
    {
        public ConnectionsView()
        {
            InitializeComponent();
        }
    }
}
