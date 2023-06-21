using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    [ExportView(typeof(FlightUavViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class FlightUavView : ReactiveUserControl<FlightUavViewModel>
    {
        public FlightUavView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}