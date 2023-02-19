using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
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
