using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(SerialPortViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SerialPortView : ReactiveUserControl<SerialPortViewModel>
    {
        public SerialPortView()
        {
            InitializeComponent();
        }
    }
}
