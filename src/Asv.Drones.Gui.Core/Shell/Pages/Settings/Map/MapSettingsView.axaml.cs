using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(MapSettingsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MapSettingsView : ReactiveUserControl<MapSettingsViewModel>
    {
        public MapSettingsView()
        {
            InitializeComponent();
        }
    }
}
