using System.ComponentModel.Composition;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(MeasureUnitsSettingsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MeasureUnitsSettingsView : ReactiveUserControl<MeasureUnitsSettingsViewModel>
    {
        public MeasureUnitsSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }    
}

