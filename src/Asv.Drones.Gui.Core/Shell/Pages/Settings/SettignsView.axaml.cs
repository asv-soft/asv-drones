using System.ComponentModel.Composition;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(SettingsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
