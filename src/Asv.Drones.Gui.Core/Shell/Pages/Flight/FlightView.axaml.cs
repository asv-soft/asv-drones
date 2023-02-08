using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(FlightViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class FlightView : ReactiveUserControl<FlightViewModel>
    {
        public FlightView()
        {
            InitializeComponent();
        }
    }
}
