using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(SerialPortViewModel))]
    public partial class SerialPortView : ReactiveUserControl<SerialPortViewModel>
    {
        public SerialPortView()
        {
            InitializeComponent();
        }
    }
}