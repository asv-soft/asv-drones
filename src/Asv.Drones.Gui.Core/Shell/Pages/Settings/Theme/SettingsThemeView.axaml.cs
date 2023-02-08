using System.ComponentModel.Composition;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(SettingsThemeViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SettingsThemeView : ReactiveUserControl<SettingsThemeViewModel>
    {
        public SettingsThemeView()
        {
            InitializeComponent();
        }
    }
}
