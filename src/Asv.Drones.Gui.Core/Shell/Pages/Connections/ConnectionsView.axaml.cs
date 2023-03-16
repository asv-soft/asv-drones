using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
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
