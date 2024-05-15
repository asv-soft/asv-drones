using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(ShellStatusPortsViewModel))]
    public partial class ShellStatusPortsView : ReactiveUserControl<ShellStatusPortsViewModel>
    {
        public ShellStatusPortsView()
        {
            InitializeComponent();
        }
    }
}