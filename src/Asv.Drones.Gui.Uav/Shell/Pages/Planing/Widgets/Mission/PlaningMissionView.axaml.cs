using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(PlaningMissionViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PlaningMissionView : ReactiveUserControl<PlaningMissionViewModel>
    {
        public PlaningMissionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}