using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(TcpPortViewModel))]
    public partial class TcpPortView : ReactiveUserControl<TcpPortViewModel>
    {
        public TcpPortView()
        {
            InitializeComponent();
        }
    }
}