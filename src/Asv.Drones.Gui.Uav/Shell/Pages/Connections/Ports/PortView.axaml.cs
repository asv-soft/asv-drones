using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(PortViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PortView : ReactiveUserControl<PortViewModel>
    {
        public PortView()
        {
            InitializeComponent();
        }
    }
}
