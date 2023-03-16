using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
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
