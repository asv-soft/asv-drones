using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
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
