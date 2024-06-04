using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui
{
    [ExportView(typeof(MavlinkIdViewModel))]
    public partial class MavlinkIdView : ReactiveUserControl<MavlinkIdViewModel>
    {
        public MavlinkIdView()
        {
            InitializeComponent();
        }
    }
}