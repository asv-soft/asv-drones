using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(PortViewModel))]
    public partial class PortView : ReactiveUserControl<PortViewModel>
    {
        public PortView()
        {
            InitializeComponent();
        }
    }
}