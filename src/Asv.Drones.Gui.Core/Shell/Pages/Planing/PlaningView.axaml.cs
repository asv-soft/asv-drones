using System.ComponentModel.Composition;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(PlaningViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class PlaningView : ReactiveUserControl<PlaningViewModel>
    {
        public PlaningView()
        {
            InitializeComponent();
        }
    }
}
