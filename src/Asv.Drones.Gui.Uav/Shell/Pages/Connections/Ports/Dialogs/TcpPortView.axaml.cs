using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(TcpPortViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TcpPortView : ReactiveUserControl<TcpPortViewModel>
    {
        public TcpPortView()
        {
            InitializeComponent();
        }
    }
}
