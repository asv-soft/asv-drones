using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
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
