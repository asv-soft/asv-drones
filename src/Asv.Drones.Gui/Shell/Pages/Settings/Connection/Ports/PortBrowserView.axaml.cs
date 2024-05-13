using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(PortBrowserViewModel))]
    public partial class PortBrowserView : ReactiveUserControl<PortBrowserViewModel>
    {
        public PortBrowserView()
        {
            InitializeComponent();
        }
    }
}